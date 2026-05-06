using CMSXData.Models;

namespace ICMSX;

public interface IUsuarioRepositorio
{
    Task<IEnumerable<object>> ListaTodosAsync();
    Task<IEnumerable<object>> ListaPorAplicacaoAsync(string aplicacaoid);
    Task<Usuario?> BuscaPorIdAsync(string id);
    Task<bool> PertenceAplicacaoAsync(string userid, string aplicacaoid);
    Task CriarAsync(Usuario usuario, Relusuarioaplicacao? vinculo);
    Task AtualizarAsync(Usuario usuario);
    Task RemoverAsync(Usuario usuario);
}
