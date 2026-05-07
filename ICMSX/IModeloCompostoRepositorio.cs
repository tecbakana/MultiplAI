using CMSXData.Models;

namespace ICMSX;

public interface IModeloCompostoRepositorio
{
    Task<IEnumerable<ModeloComposto>> ListarPorProdutoAsync(string aplicacaoid, string produtoid);
    Task<ModeloComposto?> BuscarPorHashAsync(string hash, string aplicacaoid, string produtoid);
    Task CriarOuIncrementarAsync(ModeloComposto modelo, IEnumerable<ModeloSelecao> selecoes);
}
