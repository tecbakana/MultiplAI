using CMSXData.Models;

namespace ICMSX;

public interface IModuloRepositorio
{
    Task<IEnumerable<Modulo>> ListaTodosAsync();
    Task<IEnumerable<Modulo>> ListaPorAplicacaoAsync(string aplicacaoid);
    Task<IEnumerable<Modulo>> ListaPorUsuarioAsync(string usuarioid);
    Task<Modulo?> BuscaPorIdAsync(string moduloid);
    Task CriarAsync(Modulo modulo);
    Task AtualizarAsync(Modulo modulo);
    Task RemoverAsync(Modulo modulo);
}
