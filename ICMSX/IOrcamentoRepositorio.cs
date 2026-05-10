using CMSXData.Models;

namespace ICMSX;

public interface IOrcamentoRepositorio
{
    Task<IEnumerable<OrcamentoCabecalho>> ListaAsync(string aplicacaoid);
    Task<OrcamentoCabecalho?> BuscaPorIdAsync(Guid id);
    Task<Guid> CriarAsync(OrcamentoInput input);
    Task<bool> ToggleAprovadoAsync(Guid id);
    Task<bool> RemoveAsync(Guid id);
    Task<IEnumerable<Produto>> ListaProdutosPublicosAsync(string aplicacaoid);
}
