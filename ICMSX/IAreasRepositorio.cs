using CMSXData.Models;

namespace ICMSX;

public interface IAreasRepositorio
{
    IEnumerable<Area> Lista(string? aplicacaoid);
    Area? BuscaPorId(string id);
    void Criar(Area area);
    void Atualizar(Area area);
    void Remover(Area area);
}
