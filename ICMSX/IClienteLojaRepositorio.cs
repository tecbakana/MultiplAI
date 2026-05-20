using CMSXData.Models;

namespace ICMSX;

public interface IClienteLojaRepositorio
{
    Task CriaClienteLojaAsync(CriaClienteLojaInput cliente);
    Task<IEnumerable<ClienteLoja>> ListaClienteLojaAsync();

    public record CriaClienteLojaInput(string Aplicacaoid, int salematicClienteId);
}
