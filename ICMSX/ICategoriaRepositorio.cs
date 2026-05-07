using CMSXData.Models;

namespace ICMSX;

public interface ICategoriaRepositorio
{
    Task<IEnumerable<Caterium>> ListaAsync(string? aplicacaoid);
    Task<Caterium?> BuscaPorIdAsync(string id);
    Task CriarAsync(Caterium item);
    Task AtualizarAsync(Caterium item);
    Task RemoverAsync(Caterium item);
}
