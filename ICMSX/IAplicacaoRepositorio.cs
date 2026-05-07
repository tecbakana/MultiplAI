using CMSXData.Models;

namespace ICMSX;

public interface IAplicacaoRepositorio
{
    Task<IEnumerable<Aplicacao>> ListaAsync(string? aplicacaoid);
    Task<Aplicacao?> BuscaPorIdAsync(string id);
    Task<LayoutTemplate?> BuscaTemplatePadraoAsync();
    Task CriarAsync(Aplicacao aplicacao, Area homeArea);
    Task AtualizarAsync(Aplicacao aplicacao);
    Task AlterarStatusAsync(Aplicacao aplicacao, bool ativo);
    Task RemoverAsync(Aplicacao aplicacao);
}
