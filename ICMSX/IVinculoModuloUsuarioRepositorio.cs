using CMSXData.Models;

namespace ICMSX;

public interface IVinculoModuloUsuarioRepositorio
{
    Task<IEnumerable<object>> ListaAsync(string? aplicacaoid, string? usuarioid);
    Task<bool> ExisteVinculoAsync(string usuarioid, string moduloid);
    Task CriarAsync(Relmodulousuario rel);
    Task<Relmodulousuario?> BuscaPorRelacaoidAsync(string relacaoid);
    Task RemoverAsync(Relmodulousuario rel);
}
