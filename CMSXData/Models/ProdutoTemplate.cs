namespace CMSXData.Models;

public partial class ProdutoTemplate
{
    public string ProdutoTemplateid { get; set; } = null!;
    public string Aplicacaoid { get; set; } = null!;
    public string? SegmentoTenantId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public string ConteudoJson { get; set; } = null!;
    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; } = true;
}
