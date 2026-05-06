using CMSXData.Models;

namespace ICMSX;

public record ArvoreAtributos(IEnumerable<Atributo> Atributos, IEnumerable<Opcao> Opcoes);

public interface IProdutosRepositorio
{
    // Produto
    IEnumerable<Produto> Lista(string? aplicacaoid);
    Produto? BuscaPorId(string id);
    void Criar(Produto produto);
    void Atualizar(Produto produto);
    void Remover(Produto produto);

    // Atributos
    ArvoreAtributos BuscaArvoreComOpcoes(string produtoid);
    Atributo? BuscaAtributo(Guid id);
    void CriarAtributo(Atributo atributo);
    void AtualizarAtributo(Atributo atributo);
    void RemoverAtributoComDescendentes(Guid id);

    // Opcoes
    Opcao? BuscaOpcao(string opcaoid, Guid atributoid);
    void CriarOpcao(Opcao opcao);
    void AtualizarOpcao(Opcao opcao);
    void RemoverOpcao(Opcao opcao);

    // Imagens
    IEnumerable<Imagem> ListaImagensPorProduto(string produtoid);
    Imagem? BuscaImagem(string imagemid, string produtoid);
    void CriarImagem(Imagem imagem);
    void RemoverImagem(Imagem imagem);
}
