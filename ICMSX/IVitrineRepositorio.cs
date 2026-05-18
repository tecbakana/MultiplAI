namespace ICMSX;

public record VitrineGerarInput(string Prompt, string? ImagemBase64, string? SegmentoTenantId, string? Provedor);
public record VitrineGerarAreaInput(
    string Prompt,
    string? Segmento = null,
    string? Provedor = null,
    string? Tipo = null,
    string? Estilo = null,
    string? Paleta = null,
    string? TemaCanonicoJson = null);

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

// ValoresJson normativo:
// {
//   "variaveis": { "cor-primaria": "#3498db", "fonte-titulo": "Montserrat" },
//   "slots": {
//     "hero-titulo": [ { "tipo": "texto", "config": { "texto": "Bem-vindo", "tag": "h1" }, "ordem": 0 } ],
//     "hero-cta":    [ { "tipo": "cta",   "config": { "texto": "Saiba mais", "url": "#", "variante": "primario" }, "ordem": 0 } ],
//     "produtos-grid": []
//   }
// }
// Tipos de bloco válidos: "texto" | "imagem" | "cta" | "lista"
// Configs por tipo:
//   texto  → { "texto": string, "tag": "p"|"h1"|"h2"|"h3" }
//   imagem → { "url": string, "alt": string }
//   cta    → { "texto": string, "url": string, "variante": "primario"|"secundario"|"outline" }
//   lista  → { "itens": string[] }
public record VitrineAreaConfigResumo(
    Guid? VitrineTemplateId,
    string? TemplateHtmlCss,
    string? TemplateVariaveisJson,
    string? ValoresJson,
    bool Publicado);

public record VitrineAreaConfigInput(
    Guid VitrineTemplateId,
    string ValoresJson);

public interface IVitrineRepositorio
{
    // Admin
    Task<IEnumerable<VitrineTemplateResumo>> ListaTemplatesAsync();
    ValueTask<VitrineTemplateDetalhe?> BuscaTemplateAsync(Guid id);
    Task<Guid> CriarTemplateAsync(VitrineTemplateInput input);
    Task<bool> AtualizarTemplateAsync(Guid id, VitrineTemplateInput input);
    Task<bool> DesativarTemplateAsync(Guid id);

    // Tenant — aplicação (VitrineConfigurada, por aplicação/home)
    Task<IEnumerable<VitrineTemplateResumo>> ListaTemplatesDisponiveisAsync(string aplicacaoId);
    ValueTask<VitrineConfiguradaResumo?> BuscaConfiguradaAsync(string aplicacaoId);
    Task<Guid> SalvarConfiguradaAsync(string aplicacaoId, VitrineConfiguradaInput input);
    Task<bool> PublicarAsync(string aplicacaoId, string htmlSnapshot);
    Task<string?> BuscaSnapshotAsync(string aplicacaoId);
    Task<string?> BuscaCssProcessadoAsync(Guid vitrineConfiguradaId);
    Task<string?> RenderAsync(string aplicacaoId, string? conteudoPb = null, string? navHtml = null);
    Task<string?> RenderComDadosAsync(string aplicacaoId, ISiteRepositorio siteRepo, string? navHtml = null, string? conteudoPb = null);

    // Tenant — área (Vitrine por área, independente)
    // RenderAreaAsync: busca Area → busca VitrineTemplate → deserializa variaveis/slots →
    //   aplica variaveis como CSS custom properties → substitui data-vitrine-slot no HTML → retorna HTML autocontido
    ValueTask<VitrineAreaConfigResumo?> BuscaAreaConfigAsync(string areaId);
    Task SalvarAreaConfigAsync(string areaId, VitrineAreaConfigInput input);
    Task<bool> PublicarAreaAsync(string areaId, string htmlSnapshot);
    Task<string?> RenderAreaAsync(string areaId, ISiteRepositorio siteRepo);
    Task<string?> BuscaAreaSnapshotAsync(string areaId);
    Task<Dictionary<string, string?>> BuscaSnapshotsPorAreasAsync(IEnumerable<string> areaIds);

    // Schema-driven (nova engine AI → VitrineConfig → renderer)
    Task SalvarAreaSchemaAsync(string areaId, VitrineConfig config);
    ValueTask<VitrineConfig?> BuscaAreaSchemaAsync(string areaId);
    Task<string?> RenderAreaSchemaAsync(string areaId, ISiteRepositorio siteRepo, string? cssContent = null);
}
