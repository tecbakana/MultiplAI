using CMSXData.Models;

namespace ICMSX;

public interface IAtributoRepositorio
{
    Task MakeConnectionAsync(dynamic prop);
    Task<List<Atributo>> ListaAtributoAsync();
    Task<List<Atributo>> ListaAtributoXProdutoAsync();
    Task CriaAtributoAsync(Atributo at);
    Task InativaAtributoAsync();
    Task<List<Atributo>> ListaAtributosArvoreAsync(IEnumerable<string> produtoIds);
}