using CMSXData.Models;

namespace ICMSX;

public interface ILayoutTemplateRepositorio
{
    IEnumerable<LayoutTemplate> Lista();
    LayoutTemplate? BuscaPorId(string id);
    LayoutTemplate? BuscaPadrao(string tipo);
    void DesmarcarPadraoDoTipo(string tipo, string? excluirId);
    void Criar(LayoutTemplate template);
    void Atualizar(LayoutTemplate template);
    void Remover(LayoutTemplate template);
}
