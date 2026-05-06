using CMSXData.Models;

namespace ICMSX;

public interface IOrcamentoRepositorio
{
    Task<IEnumerable<OrcamentoCabecalho>> ListaAsync(string aplicacaoid);
    Task<OrcamentoCabecalho?> BuscaPorIdAsync(Guid id);
    Task CriarAsync(OrcamentoCabecalho cabecalho, IEnumerable<OrcamentoDetalhe> itens);
    Task<IEnumerable<Produto>> ListaProdutosPublicosAsync(string aplicacaoid);
    Task ToggleAprovadoAsync(OrcamentoCabecalho orcamento);
    Task RemoveAsync(OrcamentoCabecalho orcamento);
}
