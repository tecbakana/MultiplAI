using CMSXData.Models;

namespace ICMSX;

public interface IOrcamentoRepositorio
{
    Task<IEnumerable<OrcamentoCabecalho>> ListaAsync(string aplicacaoid);
    Task<OrcamentoCabecalho?> BuscaPorIdAsync(Guid id, string? aplicacaoid = null);
    Task<Guid> CriarAsync(OrcamentoInput input);
    Task<bool?> ToggleAprovadoAsync(Guid id, string? aplicacaoid = null);
    Task<bool> RemoveAsync(Guid id, string? aplicacaoid = null);
    Task<IEnumerable<Produto>> ListaProdutosPublicosAsync(string aplicacaoid);
}
