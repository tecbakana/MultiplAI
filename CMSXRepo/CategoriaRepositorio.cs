using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class CategoriaRepositorio : BaseRepositorio, ICategoriaRepositorio
{
    public CategoriaRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Caterium>> ListaAsync(string? aplicacaoid) =>
        string.IsNullOrEmpty(aplicacaoid)
            ? await _db.Cateria.AsNoTracking().OrderBy(c => c.Nome).ToListAsync()
            : await _db.Cateria.AsNoTracking().Where(c => c.Aplicacaoid == aplicacaoid).OrderBy(c => c.Nome).ToListAsync();

    public async Task<Caterium?> BuscaPorIdAsync(string id) =>
        await _db.Cateria.AsNoTracking().FirstOrDefaultAsync(c => c.Cateriaid == id);

    public async Task CriarAsync(Caterium item)
    {
        _db.Cateria.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Caterium item)
    {
        _db.Cateria.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Caterium item)
    {
        _db.Cateria.Remove(item);
        await _db.SaveChangesAsync();
    }
}
