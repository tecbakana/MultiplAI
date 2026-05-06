using CMSXData.Models;

namespace ICMSX;

public interface IUsuarioRepositorio
{
    IEnumerable<object> ListaTodos();
    IEnumerable<object> ListaPorAplicacao(string aplicacaoid);
    Usuario? BuscaPorId(string id);
    bool PertenceAplicacao(string userid, string aplicacaoid);
    void Criar(Usuario usuario, Relusuarioaplicacao? vinculo);
    void Atualizar(Usuario usuario);
    void Remover(Usuario usuario);
}
