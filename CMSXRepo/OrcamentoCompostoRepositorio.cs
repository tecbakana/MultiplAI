using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class OrcamentoCompostoRepositorio : BaseRepositorio, IOrcamentoCompostoRepositorio
{
    public OrcamentoCompostoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<OrcamentoDetalheComposto>> ListarAtuaisAsync(Guid orcamentoid) =>
        await _db.OrcamentoDetalheCompostos
            .Include(d => d.Selecoes)
            .Where(d => d.Orcamentoid == orcamentoid && d.Atual)
            .ToListAsync();

    public async Task<Produto?> BuscarProdutoAsync(string produtoid) =>
        await _db.Produtos.FirstOrDefaultAsync(p => p.Produtoid == produtoid);

    public async Task<IEnumerable<Opcao>> BuscarOpcoesAsync(IEnumerable<string> opcaoIds)
    {
        var ids = opcaoIds.ToList();
        return await _db.Opcaos.Where(o => ids.Contains(o.Opcaoid)).ToListAsync();
    }

    public async Task<IEnumerable<Atributo>> BuscarAtributosAsync(IEnumerable<Guid> atributoIds)
    {
        var ids = atributoIds.ToList();
        return await _db.Atributos.Where(a => ids.Contains(a.Atributoid)).ToListAsync();
    }

    public async Task CriarAsync(OrcamentoDetalheComposto detalhe, IEnumerable<Selecao> selecoes)
    {
        _db.OrcamentoDetalheCompostos.Add(detalhe);
        _db.Selecaos.AddRange(selecoes);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverPorOrcamentoAsync(Guid orcamentoid)
    {
        var detalhes = await _db.OrcamentoDetalheCompostos
            .Include(d => d.Selecoes)
            .Where(d => d.Orcamentoid == orcamentoid)
            .ToListAsync();

        foreach (var d in detalhes)
            _db.Selecaos.RemoveRange(d.Selecoes);

        _db.OrcamentoDetalheCompostos.RemoveRange(detalhes);
        await _db.SaveChangesAsync();
    }
}
