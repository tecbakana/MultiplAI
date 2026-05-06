using CMSXData.Models;

namespace ICMSX;

public interface IVinculoUsuarioAplicacaoRepositorio
{
    IEnumerable<object> Lista(string? aplicacaoid, string? usuarioid);
    bool ExisteVinculo(string usuarioid, string aplicacaoid);
    void Criar(Relusuarioaplicacao rel);
    Relusuarioaplicacao? BuscaPorRelacaoid(string relacaoid);
    void Remover(Relusuarioaplicacao rel);
}
