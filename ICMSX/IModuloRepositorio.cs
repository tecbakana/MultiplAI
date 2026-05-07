using CMSXData.Models;

namespace ICMSX;

public interface IModuloRepositorio
{
    Task<IEnumerable<Modulo>> ListaTodosAsync();
    Task<IEnumerable<Modulo>> ListaPorAplicacaoAsync(string aplicacaoid);
    Task<IEnumerable<Modulo>> ListaPorUsuarioAsync(string usuarioid);
}
