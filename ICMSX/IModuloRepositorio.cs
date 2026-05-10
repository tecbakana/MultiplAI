using CMSXData.Models;

namespace ICMSX;

public interface IModuloRepositorio
{
    Task<IEnumerable<Modulo>> ListaTodosAsync();
    Task<IEnumerable<Modulo>> ListaPorAplicacaoAsync(string aplicacaoid);
    Task<IEnumerable<Modulo>> ListaPorUsuarioAsync(string usuarioid);
    Task<Modulo?> BuscaPorIdAsync(string moduloid);
    Task<string> CriarAsync(ModuloInput input);
    Task<bool> AtualizarAsync(string id, ModuloInput input);
    Task<bool> RemoverAsync(string id);
}
