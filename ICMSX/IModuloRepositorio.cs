using CMSXData.Models;

namespace ICMSX;

public interface IModuloRepositorio
{
    IEnumerable<Modulo> ListaTodos();
    IEnumerable<Modulo> ListaPorAplicacao(string aplicacaoid);
    IEnumerable<Modulo> ListaPorUsuario(string usuarioid);
}
