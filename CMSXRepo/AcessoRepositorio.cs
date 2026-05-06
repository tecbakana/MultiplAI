using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class AcessoRepositorio : BaseRepositorio, IAcessoRepositorio
{
    public AcessoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<bool> ApelidoDisponivelAsync(string apelido) =>
        !await _db.Usuarios.AnyAsync(u => u.Apelido == apelido);

    public async Task<bool> UrlDisponivelAsync(string url) =>
        !await _db.Aplicacaos.AnyAsync(a => a.Url == url);

    public async Task CriarContaAsync(Usuario usuario, Aplicacao aplicacao, Relusuarioaplicacao relacao)
    {
        _db.Usuarios.Add(usuario);
        _db.Aplicacaos.Add(aplicacao);
        _db.Relusuarioaplicacaos.Add(relacao);
        await _db.SaveChangesAsync();
    }

    public async Task<LoginResultado?> LoginAsync(string apelido, string senha)
    {
        var user = await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Apelido == apelido && u.Senha == senha && u.Ativo == (byte)1);
        if (user == null) return null;

        var grupos = await _db.Relusuariogrupos.AsNoTracking()
            .Where(r => r.Usuarioid == user.Userid)
            .Join(_db.Grupos.AsNoTracking(), r => r.Grupoid, g => g.Grupoid, (r, g) => g)
            .ToListAsync();

        bool acessoTotal = grupos.Any(g => g.Acessototal);
        var nomesGrupos  = grupos.Select(g => g.Nome ?? "").ToList();

        var aplicacaoid = await _db.Relusuarioaplicacaos.AsNoTracking()
            .Where(r => r.Usuarioid == user.Userid)
            .Select(r => r.Aplicacaoid)
            .FirstOrDefaultAsync();

        var app    = await _db.Aplicacaos.AsNoTracking().FirstOrDefaultAsync(a => a.Aplicacaoid == aplicacaoid);
        bool isDemo = app?.IsDemo ?? false;

        return new LoginResultado(user, acessoTotal, nomesGrupos, aplicacaoid, isDemo);
    }

    public async Task<DemoLoginResultado?> DemoLoginAsync()
    {
        var user = await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Apelido == "demo" && u.Ativo == (byte)1);
        if (user == null) return null;

        var aplicacaoid = await _db.Relusuarioaplicacaos.AsNoTracking()
            .Where(r => r.Usuarioid == user.Userid)
            .Select(r => r.Aplicacaoid)
            .FirstOrDefaultAsync();

        if (aplicacaoid == null) return null;

        return new DemoLoginResultado(user, aplicacaoid);
    }

    public async Task ResetarTenantDemoAsync(string aplicacaoid)
    {
        var areas = await _db.Areas.Where(a => a.Aplicacaoid == aplicacaoid).ToListAsync();
        foreach (var area in areas)
            area.Layout = "{\"blocos\":[]}";

        var areaIds   = areas.Select(a => a.Areaid).ToList();
        var conteudos = await _db.Conteudos.Where(c => areaIds.Contains(c.Areaid)).ToListAsync();
        _db.Conteudos.RemoveRange(conteudos);

        var hoje    = DateOnly.FromDateTime(DateTime.UtcNow);
        var usoHoje = await _db.IaUsos.Where(u => u.Aplicacaoid == aplicacaoid && u.Data == hoje).ToListAsync();
        _db.IaUsos.RemoveRange(usoHoje);

        await _db.SaveChangesAsync();
    }
}
