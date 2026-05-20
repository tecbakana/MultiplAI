namespace ICMSX;

public record SegmentoTenantInput(string Nome, string? Descricao);
public record GerarSegmentoTemplatesInput(string PromptSegmento);

public record SegmentoTenantResultado(string SegmentoTenantId, string Nome, string? Descricao, bool Ativo);

public interface ISegmentoTenantRepositorio
{
    Task<IEnumerable<SegmentoTenantResultado>> ListaAtivosAsync();
    Task<SegmentoTenantResultado?> BuscaPorIdAsync(string id);
    Task<string> CriarAsync(SegmentoTenantInput input);
    Task<bool> AtualizarAsync(string id, SegmentoTenantInput input);
    Task<bool> RemoverAsync(string id);

    Task<IEnumerable<SegmentoTenantResultado>> ListaPorAplicacaoAsync(string aplicacaoid);
    Task VincularAsync(string aplicacaoid, string segmentoTenantId);
    Task DesvincularAsync(string aplicacaoid, string segmentoTenantId);
    Task<IEnumerable<string>> ListaSegmentoIdsPorAplicacaoAsync(string aplicacaoid);
}
