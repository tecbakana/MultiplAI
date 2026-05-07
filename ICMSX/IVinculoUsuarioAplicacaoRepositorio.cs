using CMSXData.Models;

namespace ICMSX;

public interface IVinculoUsuarioAplicacaoRepositorio
{
    Task<IEnumerable<object>> ListaAsync(string? aplicacaoid, string? usuarioid);
    Task<bool> ExisteVinculoAsync(string usuarioid, string aplicacaoid);
    Task CriarAsync(Relusuarioaplicacao rel);
    Task<Relusuarioaplicacao?> BuscaPorRelacaoidAsync(string relacaoid);
    Task RemoverAsync(Relusuarioaplicacao rel);
}
