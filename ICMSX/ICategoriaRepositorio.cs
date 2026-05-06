using CMSXData.Models;

namespace ICMSX;

public interface ICategoriaRepositorio
{
    IEnumerable<Caterium> Lista(string? aplicacaoid);
    Caterium? BuscaPorId(string id);
    void Criar(Caterium item);
    void Atualizar(Caterium item);
    void Remover(Caterium item);
}
