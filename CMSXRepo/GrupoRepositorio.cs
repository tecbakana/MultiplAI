using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class GrupoRepositorio : BaseRepositorio, IGrupoRepositorio
{
    public GrupoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Grupo>> ListaAsync() =>
        await _db.Grupos.AsNoTracking().OrderBy(g => g.Nome).ToListAsync();

    public async Task<Grupo?> BuscaPorIdAsync(string id) =>
        await _db.Grupos.AsNoTracking().FirstOrDefaultAsync(g => g.Grupoid == id);

    public async Task<IEnumerable<object>> UsuariosPorGrupoAsync(string grupoid) =>
        await _db.Relusuariogrupos.AsNoTracking()
            .Where(r => r.Grupoid == grupoid)
            .Join(_db.Usuarios.AsNoTracking(), r => r.Usuarioid, u => u.Userid,
                (r, u) => (object)new { r.Relacaoid, u.Userid, u.Nome, u.Sobrenome, u.Apelido, u.Ativo })
            .ToListAsync();

    public async Task<bool> ExisteVinculoUsuarioAsync(string grupoid, string usuarioid) =>
        await _db.Relusuariogrupos.AnyAsync(r => r.Grupoid == grupoid && r.Usuarioid == usuarioid);

    public async Task CriarAsync(Grupo grupo)
    {
        _db.Grupos.Add(grupo);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Grupo grupo)
    {
        _db.Grupos.Update(grupo);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverComVinculosAsync(Grupo grupo)
    {
        var vinculos = await _db.Relusuariogrupos.Where(r => r.Grupoid == grupo.Grupoid).ToListAsync();
        _db.Relusuariogrupos.RemoveRange(vinculos);
        _db.Grupos.Remove(grupo);
        await _db.SaveChangesAsync();
    }

    public async Task AdicionarUsuarioAsync(Relusuariogrupo rel)
    {
        _db.Relusuariogrupos.Add(rel);
        await _db.SaveChangesAsync();
    }

    public async Task<Relusuariogrupo?> BuscaVinculoPorRelacaoidAsync(string relacaoid) =>
        await _db.Relusuariogrupos.FirstOrDefaultAsync(r => r.Relacaoid == relacaoid);

    public async Task RemoverVinculoUsuarioAsync(Relusuariogrupo rel)
    {
        _db.Relusuariogrupos.Remove(rel);
        await _db.SaveChangesAsync();
    }
}
