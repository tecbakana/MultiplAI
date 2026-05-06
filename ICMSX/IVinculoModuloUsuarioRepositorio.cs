using CMSXData.Models;

namespace ICMSX;

public interface IVinculoModuloUsuarioRepositorio
{
    IEnumerable<object> Lista(string? aplicacaoid, string? usuarioid);
    bool ExisteVinculo(string usuarioid, string moduloid);
    void Criar(Relmodulousuario rel);
    Relmodulousuario? BuscaPorRelacaoid(string relacaoid);
    void Remover(Relmodulousuario rel);
}
