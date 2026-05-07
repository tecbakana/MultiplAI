using CMSXData.Models;

namespace ICMSX;

public interface IOrcamentoCompostoRepositorio
{
    Task<IEnumerable<OrcamentoDetalheComposto>> ListarAtuaisAsync(Guid orcamentoid);
    Task<Produto?> BuscarProdutoAsync(string produtoid);
    Task<IEnumerable<Opcao>> BuscarOpcoesAsync(IEnumerable<string> opcaoIds);
    Task<IEnumerable<Atributo>> BuscarAtributosAsync(IEnumerable<Guid> atributoIds);
    Task CriarAsync(OrcamentoDetalheComposto detalhe, IEnumerable<Selecao> selecoes);
    Task RemoverPorOrcamentoAsync(Guid orcamentoid);
}
