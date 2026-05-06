using CMSXData.Models;

namespace ICMSX;

public interface ILayoutTemplateRepositorio
{
    Task<IEnumerable<LayoutTemplate>> ListaAsync();
    Task<LayoutTemplate?> BuscaPorIdAsync(string id);
    Task<LayoutTemplate?> BuscaPadraoAsync(string tipo);
    Task DesmarcarPadraoDoTipoAsync(string tipo, string? excluirId);
    Task CriarAsync(LayoutTemplate template);
    Task AtualizarAsync(LayoutTemplate template);
    Task RemoverAsync(LayoutTemplate template);
}
