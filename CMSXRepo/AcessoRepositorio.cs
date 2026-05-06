using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class AcessoRepositorio : BaseRepositorio, IAcessoRepositorio
{
    public AcessoRepositorio(CmsxDbContext db) : base(db) { }

    public bool ApelidoDisponivel(string apelido) =>
        !_db.Usuarios.Any(u => u.Apelido == apelido);

    public bool UrlDisponivel(string url) =>
        !_db.Aplicacaos.Any(a => a.Url == url);

    public void CriarConta(Usuario usuario, Aplicacao aplicacao, Relusuarioaplicacao relacao)
    {
        _db.Usuarios.Add(usuario);
        _db.Aplicacaos.Add(aplicacao);
        _db.Relusuarioaplicacaos.Add(relacao);
        _db.SaveChanges();
    }

    public LoginResultado? Login(string apelido, string senha)
    {
        var user = _db.Usuarios.AsNoTracking()
            .FirstOrDefault(u => u.Apelido == apelido && u.Senha == senha && u.Ativo == (byte)1);
        if (user == null) return null;

        var grupos = _db.Relusuariogrupos.AsNoTracking()
            .Where(r => r.Usuarioid == user.Userid)
            .Join(_db.Grupos.AsNoTracking(), r => r.Grupoid, g => g.Grupoid, (r, g) => g)
            .ToList();

        bool acessoTotal = grupos.Any(g => g.Acessototal);
        var nomesGrupos  = grupos.Select(g => g.Nome ?? "").ToList();

        var aplicacaoid = _db.Relusuarioaplicacaos.AsNoTracking()
            .Where(r => r.Usuarioid == user.Userid)
            .Select(r => r.Aplicacaoid)
            .FirstOrDefault();

        var app    = _db.Aplicacaos.AsNoTracking().FirstOrDefault(a => a.Aplicacaoid == aplicacaoid);
        bool isDemo = app?.IsDemo ?? false;

        return new LoginResultado(user, acessoTotal, nomesGrupos, aplicacaoid, isDemo);
    }

    public DemoLoginResultado? DemoLogin()
    {
        var user = _db.Usuarios.AsNoTracking()
            .FirstOrDefault(u => u.Apelido == "demo" && u.Ativo == (byte)1);
        if (user == null) return null;

        var aplicacaoid = _db.Relusuarioaplicacaos.AsNoTracking()
            .Where(r => r.Usuarioid == user.Userid)
            .Select(r => r.Aplicacaoid)
            .FirstOrDefault();

        if (aplicacaoid == null) return null;

        return new DemoLoginResultado(user, aplicacaoid);
    }

    public void ResetarTenantDemo(string aplicacaoid)
    {
        var areas = _db.Areas.Where(a => a.Aplicacaoid == aplicacaoid).ToList();
        foreach (var area in areas)
            area.Layout = "{\"blocos\":[]}";

        var areaIds   = areas.Select(a => a.Areaid).ToList();
        var conteudos = _db.Conteudos.Where(c => areaIds.Contains(c.Areaid)).ToList();
        _db.Conteudos.RemoveRange(conteudos);

        var hoje    = DateOnly.FromDateTime(DateTime.UtcNow);
        var usoHoje = _db.IaUsos.Where(u => u.Aplicacaoid == aplicacaoid && u.Data == hoje).ToList();
        _db.IaUsos.RemoveRange(usoHoje);

        _db.SaveChanges();
    }
}
