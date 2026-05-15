namespace ICMSX;

public record VitrineGerarInput(string Prompt, string? ImagemBase64, string? SegmentoTenantId);

public record VitrineTemplateInput(
    string Nome,
    string? Descricao,
    string? SegmentoTenantId,
    string HtmlCss,
    string VariaveisJson,
    string? ThumbnailUrl);

public record VitrineTemplateResumo(
    Guid VitrineTemplateId,
    string Nome,
    string? Descricao,
    string? SegmentoTenantId,
    string VariaveisJson,
    string? ThumbnailUrl,
    DateTime DataCriacao);

public record VitrineTemplateDetalhe(
    Guid VitrineTemplateId,
    string Nome,
    string? Descricao,
    string? SegmentoTenantId,
    string HtmlCss,
    string VariaveisJson,
    string? ThumbnailUrl,
    DateTime DataCriacao);

public record VitrineConfiguradaInput(Guid VitrineTemplateId, string ValoresJson);

public record VitrineConfiguradaResumo(
    Guid VitrineConfiguradaId,
    string AplicacaoId,
    Guid VitrineTemplateId,
    string ValoresJson,
    bool Publicado);

public interface IVitrineRepositorio
{
    // Admin
    Task<IEnumerable<VitrineTemplateResumo>> ListaTemplatesAsync();
    ValueTask<VitrineTemplateDetalhe?> BuscaTemplateAsync(Guid id);
    Task<Guid> CriarTemplateAsync(VitrineTemplateInput input);
    Task<bool> AtualizarTemplateAsync(Guid id, VitrineTemplateInput input);
    Task<bool> DesativarTemplateAsync(Guid id);

    // Tenant
    Task<IEnumerable<VitrineTemplateResumo>> ListaTemplatesDisponiveisAsync(string aplicacaoId);
    ValueTask<VitrineConfiguradaResumo?> BuscaConfiguradaAsync(string aplicacaoId);
    Task<Guid> SalvarConfiguradaAsync(string aplicacaoId, VitrineConfiguradaInput input);
    Task<bool> PublicarAsync(string aplicacaoId, string htmlSnapshot);
    Task<string?> BuscaSnapshotAsync(string aplicacaoId);
    Task<string?> BuscaCssProcessadoAsync(Guid vitrineConfiguradaId);
    Task<string?> RenderAsync(string aplicacaoId, string? conteudoPb = null, string? navHtml = null);
    Task<string?> RenderComDadosAsync(string aplicacaoId, ISiteRepositorio siteRepo, string? navHtml = null, string? conteudoPb = null);
}
