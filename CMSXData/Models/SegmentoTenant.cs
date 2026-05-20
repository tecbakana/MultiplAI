namespace CMSXData.Models;

public partial class SegmentoTenant
{
    public string SegmentoTenantId { get; set; } = "";
    public string Nome { get; set; } = "";
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
}
