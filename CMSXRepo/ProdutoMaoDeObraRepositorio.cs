using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ProdutoMaoDeObraRepositorio : BaseRepositorio, IProdutoMaoDeObraRepositorio
{
    public ProdutoMaoDeObraRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<List<ProdutoMaoDeObra>> ListarPorProdutoAsync(string produtoid) =>
        await _db.ProdutoMaoDeObras
            .AsNoTracking()
            .Where(m => m.Produtoid == produtoid)
            .OrderBy(m => m.Descricao)
            .ToListAsync();

    public async Task<ProdutoMaoDeObra?> BuscarPorIdAsync(Guid id) =>
        await _db.ProdutoMaoDeObras.FirstOrDefaultAsync(m => m.Id == id);

    public async Task<ProdutoMaoDeObra> CriarAsync(ProdutoMaoDeObra mo)
    {
        _db.ProdutoMaoDeObras.Add(mo);
        await _db.SaveChangesAsync();
        return mo;
    }

    public async Task<ProdutoMaoDeObra> AtualizarAsync(ProdutoMaoDeObra mo)
    {
        await _db.SaveChangesAsync();
        return mo;
    }

    public async Task RemoverAsync(ProdutoMaoDeObra mo)
    {
        _db.ProdutoMaoDeObras.Remove(mo);
        await _db.SaveChangesAsync();
    }
}
