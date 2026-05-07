using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class VinculoUsuarioAplicacaoRepositorio : BaseRepositorio, IVinculoUsuarioAplicacaoRepositorio
{
    public VinculoUsuarioAplicacaoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<object>> ListaAsync(string? aplicacaoid, string? usuarioid)
    {
        var q = _db.Relusuarioaplicacaos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
            q = q.Where(r => r.Aplicacaoid == aplicacaoid);

        if (!string.IsNullOrEmpty(usuarioid))
            q = q.Where(r => r.Usuarioid == usuarioid);

        var rels = await q
            .Join(_db.Usuarios.AsNoTracking(), r => r.Usuarioid, u => u.Userid,
                (r, u) => new { r.Relacaoid, r.Aplicacaoid, r.Usuarioid, u.Nome, u.Sobrenome, u.Apelido, u.Ativo })
            .ToListAsync();

        var appIds = rels.Select(r => r.Aplicacaoid).Distinct().ToList();
        var apps = await _db.Aplicacaos.AsNoTracking()
            .Where(a => appIds.Contains(a.Aplicacaoid))
            .Select(a => new { a.Aplicacaoid, a.Nome })
            .ToListAsync();

        return rels.Join(apps, r => r.Aplicacaoid, a => a.Aplicacaoid,
            (r, a) => (object)new { r.Relacaoid, r.Usuarioid, r.Nome, r.Sobrenome, r.Apelido, r.Ativo, r.Aplicacaoid, AppNome = a.Nome })
            .ToList();
    }

    public async Task<bool> ExisteVinculoAsync(string usuarioid, string aplicacaoid) =>
        await _db.Relusuarioaplicacaos.AnyAsync(r => r.Usuarioid == usuarioid && r.Aplicacaoid == aplicacaoid);

    public async Task CriarAsync(Relusuarioaplicacao rel)
    {
        _db.Relusuarioaplicacaos.Add(rel);
        await _db.SaveChangesAsync();
    }

    public async Task<Relusuarioaplicacao?> BuscaPorRelacaoidAsync(string relacaoid) =>
        await _db.Relusuarioaplicacaos.FirstOrDefaultAsync(r => r.Relacaoid == relacaoid);

    public async Task RemoverAsync(Relusuarioaplicacao rel)
    {
        _db.Relusuarioaplicacaos.Remove(rel);
        await _db.SaveChangesAsync();
    }
}
