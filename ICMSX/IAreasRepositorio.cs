using CMSXData.Models;

namespace ICMSX;

public record AreaInput(
    string? Nome,
    string? Url,
    string? Descricao,
    string? Areaidpai,
    int? Posicao,
    int? Tipoarea,
    string Tipo,
    bool CanonicalArea = false);

public interface IAreasRepositorio
{
    Task<IEnumerable<Area>> ListaAsync(string? aplicacaoid);
    Task<Area?> BuscaPorIdAsync(string id);
    Task<Area?> BuscaHomeAsync(string aplicacaoid);
    Task<bool> ExisteHomeAsync(string aplicacaoid, string? excluirAreaId = null);
    Task<string> CriarAsync(AreaInput input, string aplicacaoid);
    Task<bool> AtualizarAsync(string id, AreaInput input);
    Task<bool> AtualizarLayoutAsync(string id, string layout);
    Task<bool> AtualizarPageBuilderVersionAsync(string id, string version);
    Task RemoverAsync(Area area);
}
