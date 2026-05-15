namespace CMSXData.Models;

public class VitrineConfigurada
{
    public Guid VitrineConfiguradaId { get; set; }
    public string AplicacaoId { get; set; } = "";
    public Guid VitrineTemplateId { get; set; }
    public string ValoresJson { get; set; } = "";
    public bool Publicado { get; set; } = false;
    public string? HtmlSnapshot { get; set; }
    public string? CssProcessado { get; set; }
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
