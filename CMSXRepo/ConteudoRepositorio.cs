using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ConteudoRepositorio : BaseRepositorio, IConteudoRepositorio
{
    public ConteudoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Conteudo>> ListaAsync(string? aplicacaoid, string? areaid, string? cateriaid)
    {
        var q = _db.Conteudos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
        {
            var areasIds = await _db.Areas
                .AsNoTracking()
                .Where(a => a.Aplicacaoid == aplicacaoid)
                .Select(a => a.Areaid)
                .ToListAsync();
            q = q.Where(c => c.Areaid != null && areasIds.Contains(c.Areaid));
        }

        if (!string.IsNullOrEmpty(areaid))
            q = q.Where(c => c.Areaid == areaid);

        if (!string.IsNullOrEmpty(cateriaid))
            q = q.Where(c => c.Cateriaid == cateriaid);

        return await q.OrderByDescending(c => c.Datainclusao).ToListAsync();
    }

    public async Task<Conteudo?> BuscaPorIdAsync(string id) =>
        await _db.Conteudos.AsNoTracking().FirstOrDefaultAsync(c => c.Conteudoid == id);

    public async Task<string?> AplicacaoidDaAreaAsync(string? areaid)
    {
        if (string.IsNullOrEmpty(areaid)) return null;
        return await _db.Areas.AsNoTracking()
            .Where(a => a.Areaid == areaid)
            .Select(a => a.Aplicacaoid)
            .FirstOrDefaultAsync();
    }

    public async Task CriarAsync(Conteudo item)
    {
        _db.Conteudos.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Conteudo item)
    {
        _db.Conteudos.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Conteudo item)
    {
        _db.Conteudos.Remove(item);
        await _db.SaveChangesAsync();
    }
}
