namespace CMSAPI.Services;

public interface IMarketHubHttpService
{
    Task<string?> GetConfiguracoes(string tenantId);
    Task<bool> PostConfiguracao(string tenantId, string jsonBody);
    Task<string?> GetPedidos(string tenantId);
    Task<(bool encontrado, string? json)> GetPedido(string tenantId, string id);
    Task<bool> DeleteConfiguracao(string tenantId, string marketplace);
    Task<string?> EmitirNf(string tenantId, string id);
}
