using CMSXData.Models;

namespace ICMSX;

public interface IAreasRepositorio
{
    Task<IEnumerable<Area>> ListaAsync(string? aplicacaoid);
    Task<Area?> BuscaPorIdAsync(string id);
    Task CriarAsync(Area area);
    Task AtualizarAsync(Area area);
    Task RemoverAsync(Area area);
}
