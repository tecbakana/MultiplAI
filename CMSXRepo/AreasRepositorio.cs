using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class AreasRepositorio : BaseRepositorio, IAreasRepositorio
{
    public AreasRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Area>> ListaAsync(string? aplicacaoid) =>
        string.IsNullOrEmpty(aplicacaoid)
            ? await _db.Areas.AsNoTracking().OrderBy(a => a.Posicao).ToListAsync()
            : await _db.Areas.AsNoTracking().Where(a => a.Aplicacaoid == aplicacaoid).OrderBy(a => a.Posicao).ToListAsync();

    public async Task<Area?> BuscaPorIdAsync(string id) =>
        await _db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Areaid == id);

    public async Task CriarAsync(Area area)
    {
        _db.Areas.Add(area);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Area area)
    {
        _db.Areas.Update(area);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Area area)
    {
        _db.Areas.Remove(area);
        await _db.SaveChangesAsync();
    }
}
