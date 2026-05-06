using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class UsuarioRepositorio : BaseRepositorio, IUsuarioRepositorio
{
    public UsuarioRepositorio(CmsxDbContext db) : base(db) { }

    private async Task<HashSet<string?>> AdminIdsAsync() =>
        (await _db.Relusuariogrupos.AsNoTracking()
            .Join(_db.Grupos.AsNoTracking(), r => r.Grupoid, g => g.Grupoid, (r, g) => new { r.Usuarioid, g.Acessototal })
            .Where(x => x.Acessototal)
            .Select(x => x.Usuarioid)
            .ToListAsync()).ToHashSet();

    public async Task<IEnumerable<object>> ListaTodosAsync() =>
        await _db.Usuarios.AsNoTracking()
            .Select(u => (object)new { u.Userid, u.Nome, u.Sobrenome, u.Apelido, u.Ativo, u.Datainclusao })
            .ToListAsync();

    public async Task<IEnumerable<object>> ListaPorAplicacaoAsync(string aplicacaoid)
    {
        var adminIds = await AdminIdsAsync();
        return (await _db.Relusuarioaplicacaos.AsNoTracking()
            .Where(r => r.Aplicacaoid == aplicacaoid)
            .Join(_db.Usuarios.AsNoTracking(), r => r.Usuarioid, u => u.Userid,
                (r, u) => new { u.Userid, u.Nome, u.Sobrenome, u.Apelido, u.Ativo, u.Datainclusao })
            .ToListAsync())
            .Where(u => !adminIds.Contains(u.Userid))
            .Cast<object>()
            .ToList();
    }

    public async Task<Usuario?> BuscaPorIdAsync(string id) =>
        await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Userid == id);

    public async Task<bool> PertenceAplicacaoAsync(string userid, string aplicacaoid) =>
        await _db.Relusuarioaplicacaos.AnyAsync(r => r.Usuarioid == userid && r.Aplicacaoid == aplicacaoid);

    public async Task CriarAsync(Usuario usuario, Relusuarioaplicacao? vinculo)
    {
        _db.Usuarios.Add(usuario);
        if (vinculo != null)
            _db.Relusuarioaplicacaos.Add(vinculo);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Usuario usuario)
    {
        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();
    }
}
