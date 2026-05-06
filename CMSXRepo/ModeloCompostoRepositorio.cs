using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ModeloCompostoRepositorio : BaseRepositorio, IModeloCompostoRepositorio
{
    public ModeloCompostoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<ModeloComposto>> ListarPorProdutoAsync(string aplicacaoid, string produtoid) =>
        await _db.ModeloCompostos
            .AsNoTracking()
            .Where(m => m.Aplicacaoid == aplicacaoid && m.Produtoid == produtoid)
            .OrderByDescending(m => m.Usos)
            .ToListAsync();

    public async Task<ModeloComposto?> BuscarPorHashAsync(string hash, string aplicacaoid, string produtoid) =>
        await _db.ModeloCompostos.FirstOrDefaultAsync(m =>
            m.ConfiguracaoHash == hash &&
            m.Aplicacaoid == aplicacaoid &&
            m.Produtoid == produtoid);

    public async Task CriarOuIncrementarAsync(ModeloComposto modelo, IEnumerable<ModeloSelecao> selecoes)
    {
        var existente = await BuscarPorHashAsync(modelo.ConfiguracaoHash, modelo.Aplicacaoid, modelo.Produtoid);

        if (existente != null)
        {
            existente.Usos++;
            await _db.SaveChangesAsync();
            return;
        }

        _db.ModeloCompostos.Add(modelo);
        _db.ModeloSelecaos.AddRange(selecoes);
        await _db.SaveChangesAsync();
    }
}
