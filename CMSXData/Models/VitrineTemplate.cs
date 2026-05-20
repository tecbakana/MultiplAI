namespace CMSXData.Models;

public class VitrineTemplate
{
    public Guid VitrineTemplateId { get; set; }
    public string Nome { get; set; } = "";
    public string? Descricao { get; set; }
    public string? SegmentoTenantId { get; set; }
    public string HtmlCss { get; set; } = "";
    public string VariaveisJson { get; set; } = "";
    public string? ThumbnailUrl { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
}
