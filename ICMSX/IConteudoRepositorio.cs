using CMSXData.Models;

namespace ICMSX;

public interface IConteudoRepositorio
{
    Task<IEnumerable<Conteudo>> ListaAsync(string? aplicacaoid, string? areaid, string? cateriaid);
    Task<Conteudo?> BuscaPorIdAsync(string id);
    Task<string?> AplicacaoidDaAreaAsync(string? areaid);
    Task CriarAsync(Conteudo item);
    Task AtualizarAsync(Conteudo item);
    Task RemoverAsync(Conteudo item);
}
