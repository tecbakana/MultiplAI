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
    bool ApelidoDisponivel(string apelido);
    bool UrlDisponivel(string url);
    void CriarConta(Usuario usuario, Aplicacao aplicacao, Relusuarioaplicacao relacao);

    LoginResultado? Login(string apelido, string senha);
    DemoLoginResultado? DemoLogin();
    void ResetarTenantDemo(string aplicacaoid);
}
