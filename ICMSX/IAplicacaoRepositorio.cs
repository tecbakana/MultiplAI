using CMSXData.Models;

namespace ICMSX;

public interface IAplicacaoRepositorio
{
    IEnumerable<Aplicacao> Lista(string? aplicacaoid);
    Aplicacao? BuscaPorId(string id);
    LayoutTemplate? BuscaTemplatePadrao();
    void Criar(Aplicacao aplicacao, Area homeArea);
    void Atualizar(Aplicacao aplicacao);
    void AlterarStatus(Aplicacao aplicacao, bool ativo);
    void Remover(Aplicacao aplicacao);
}
