using CMSXData.Models;

namespace ICMSX;

public interface IPublicTokenRepositorio
{
    IEnumerable<PublicToken> Lista(string aplicacaoid);
    PublicToken? BuscaPorId(Guid id);
    void Criar(PublicToken token);
    void Revogar(PublicToken token);
}
