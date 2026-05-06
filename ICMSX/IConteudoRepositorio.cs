using CMSXData.Models;

namespace ICMSX;

public interface IConteudoRepositorio
{
    IEnumerable<Conteudo> Lista(string? aplicacaoid, string? areaid, string? cateriaid);
    Conteudo? BuscaPorId(string id);
    string? AplicacaoidDaArea(string? areaid);
    void Criar(Conteudo item);
    void Atualizar(Conteudo item);
    void Remover(Conteudo item);
}
