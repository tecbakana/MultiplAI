namespace ICMSX;

public record ProdutoTemplateGerarInput(string Descricao);

public record ProdutoTemplateInput(string Nome, string? Descricao, string ConteudoJson);

public record ProdutoTemplateResultado(
    string ProdutoTemplateid,
    string Aplicacaoid,
    string? SegmentoTenantId,
    string Nome,
    string? Descricao,
    string ConteudoJson,
    DateTime DataCriacao);

public interface IProdutoTemplateRepositorio
{
    Task<IEnumerable<ProdutoTemplateResultado>> ListaPorAplicacaoAsync(string aplicacaoid);
    Task<IEnumerable<ProdutoTemplateResultado>> ListaComSegmentosAsync(string aplicacaoid, IEnumerable<string> segmentoIds);
    Task<IEnumerable<ProdutoTemplateResultado>> ListaPorSegmentoAsync(string segmentoTenantId);
    Task<ProdutoTemplateResultado?> BuscaPorIdAsync(string id, string aplicacaoid);
    Task<ProdutoTemplateResultado?> BuscaAcessivelAsync(string id, string aplicacaoid, IEnumerable<string> segmentoIds);
    Task<string> CriarAsync(ProdutoTemplateInput input, string aplicacaoid);
    Task<string> CriarSegmentoAsync(ProdutoTemplateInput input, string segmentoTenantId);
    Task<bool> RemoverAsync(string id, string aplicacaoid);
    Task<bool> AplicarTemplateAsync(string produtoId, string conteudoJson);
}
