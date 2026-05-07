using CMSXData.Models;

namespace CMSAPI.Services;

public interface ISalematicHttpService
{
    Task<SalematicAuthResponse?> RegistrarAsync(RegistrarLojaRequest req);
    Task<SalematicAuthResponse?> LoginAsync(LoginLojaRequest req);
    Task<List<ClienteLoja>> ListarClientesAsync();
    Task<ClienteLoja?> CadastrarClienteAsync(ClienteLoja cliente);
}
