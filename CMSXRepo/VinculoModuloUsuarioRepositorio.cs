using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class VinculoModuloUsuarioRepositorio : BaseRepositorio, IVinculoModuloUsuarioRepositorio
{
    public VinculoModuloUsuarioRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<object>> ListaAsync(string? aplicacaoid, string? usuarioid)
    {
        List<string?> usuarioIds = !string.IsNullOrEmpty(aplicacaoid)
            ? await _db.Relusuarioaplicacaos.AsNoTracking()
                .Where(r => r.Aplicacaoid == aplicacaoid)
                .Select(r => r.Usuarioid)
                .ToListAsync()
            : await _db.Usuarios.AsNoTracking().Select(u => u.Userid).ToListAsync();

        if (!string.IsNullOrEmpty(usuarioid))
            usuarioIds = usuarioIds.Where(id => id == usuarioid).ToList();

        return await _db.Relmodulousuarios.AsNoTracking()
            .Where(r => usuarioIds.Contains(r.Usuarioid))
            .Join(_db.Usuarios.AsNoTracking(), r => r.Usuarioid, u => u.Userid,
                (r, u) => new { r.Relacaoid, r.Moduloid, r.Usuarioid, nomeUsuario = u.Nome + " " + u.Sobrenome, apelido = u.Apelido })
            .Join(_db.Modulos.AsNoTracking(), x => x.Moduloid, m => m.Moduloid,
                (x, m) => (object)new
                {
                    x.Relacaoid, x.Moduloid, x.Usuarioid,
                    x.nomeUsuario, x.apelido,
                    nomeModulo = m.Nome, urlModulo = m.Url
                })
            .ToListAsync();
    }

    public async Task<bool> ExisteVinculoAsync(string usuarioid, string moduloid) =>
        await _db.Relmodulousuarios.AnyAsync(r => r.Usuarioid == usuarioid && r.Moduloid == moduloid);

    public async Task CriarAsync(Relmodulousuario rel)
    {
        _db.Relmodulousuarios.Add(rel);
        await _db.SaveChangesAsync();
    }

    public async Task<Relmodulousuario?> BuscaPorRelacaoidAsync(string relacaoid) =>
        await _db.Relmodulousuarios.FirstOrDefaultAsync(r => r.Relacaoid == relacaoid);

    public async Task RemoverAsync(Relmodulousuario rel)
    {
        _db.Relmodulousuarios.Remove(rel);
        await _db.SaveChangesAsync();
    }
}
