using CMSXData.Models;

namespace ICMSX;

public record ArvoreAtributos(IEnumerable<Atributo> Atributos, IEnumerable<Opcao> Opcoes);

public interface IProdutosRepositorio
{
    // Produto
    Task<IEnumerable<Produto>> ListaAsync(string? aplicacaoid);
    Task<Produto?> BuscaPorIdAsync(string id);
    Task CriarAsync(Produto produto);
    Task AtualizarAsync(Produto produto);
    Task RemoverAsync(Produto produto);

    // Atributos
    Task<ArvoreAtributos> BuscaArvoreComOpcoesAsync(string produtoid);
    Task<Atributo?> BuscaAtributoAsync(Guid id);
    Task CriarAtributoAsync(Atributo atributo);
    Task AtualizarAtributoAsync(Atributo atributo);
    Task RemoverAtributoComDescendentesAsync(Guid id);

    // Opcoes
    Task<Opcao?> BuscaOpcaoAsync(string opcaoid, Guid atributoid);
    Task CriarOpcaoAsync(Opcao opcao);
    Task AtualizarOpcaoAsync(Opcao opcao);
    Task RemoverOpcaoAsync(Opcao opcao);

    // Imagens
    Task<IEnumerable<Imagem>> ListaImagensPorProdutoAsync(string produtoid);
    Task<Imagem?> BuscaImagemAsync(string imagemid, string produtoid);
    Task CriarImagemAsync(Imagem imagem);
    Task RemoverImagemAsync(Imagem imagem);
}
