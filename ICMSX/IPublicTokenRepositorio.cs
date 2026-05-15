using CMSXData.Models;

namespace ICMSX;

public interface IPublicTokenRepositorio
{
    Task<IEnumerable<PublicToken>> ListaAsync(string aplicacaoid);
    Task<PublicToken?> BuscaPorIdAsync(Guid id);
    Task<PublicToken?> BuscaPorAplicacaoAsync(string aplicacaoid);
    Task<string?> ResolveAsync(string token);
    Task CriarAsync(PublicToken token);
    Task RevogarAsync(PublicToken token);
}
