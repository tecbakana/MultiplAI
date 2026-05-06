using CMSXData.Models;

namespace ICMSX;

public interface IClienteLojaRepositorio
{
    Task MakeConnectionAsync(dynamic prop);
    Task CriaClienteLojaAsync(ClienteLoja cliente);
    Task<IEnumerable<ClienteLoja>> ListaClienteLojaAsync();
}
