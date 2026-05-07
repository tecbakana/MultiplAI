using CMSXData.Models;

namespace ICMSX;

public interface IProdutoMaoDeObraRepositorio
{
    Task<List<ProdutoMaoDeObra>> ListarPorProdutoAsync(string produtoid);
    Task<ProdutoMaoDeObra?> BuscarPorIdAsync(Guid id);
    Task<ProdutoMaoDeObra> CriarAsync(ProdutoMaoDeObra mo);
    Task<ProdutoMaoDeObra> AtualizarAsync(ProdutoMaoDeObra mo);
    Task RemoverAsync(ProdutoMaoDeObra mo);
}
