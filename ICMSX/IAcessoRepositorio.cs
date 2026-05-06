using CMSXData.Models;

namespace ICMSX;

public record LoginResultado(
    Usuario Usuario,
    bool AcessoTotal,
    IEnumerable<string> NomesGrupos,
    string? Aplicacaoid,
    bool IsDemo);

public record DemoLoginResultado(
    Usuario Usuario,
    string Aplicacaoid);

public interface IAcessoRepositorio
{
    Task<bool> ApelidoDisponivelAsync(string apelido);
    Task<bool> UrlDisponivelAsync(string url);
    Task CriarContaAsync(Usuario usuario, Aplicacao aplicacao, Relusuarioaplicacao relacao);

    Task<LoginResultado?> LoginAsync(string apelido, string senha);
    Task<DemoLoginResultado?> DemoLoginAsync();
    Task ResetarTenantDemoAsync(string aplicacaoid);
}
