using CMSXData.Models;

namespace ICMSX;

public interface IPageBuilderRepositorio
{
    Task<IEnumerable<DictBloco>> ListaBlocosAsync();
    Task<IEnumerable<string>> ListaTiposBlocosAsync();
    Task<IaConfig?> BuscaConfigAsync(string aplicacaoid);
    Task SalvarConfigAsync(string aplicacaoid, string? provedor, string? modelo, int? limiteDiario, string? apikey);
    Task RemoverApiKeyAsync(string aplicacaoid);
    Task<int> BuscaUsoHojeAsync(string aplicacaoid, DateOnly data);
    Task<IaCache?> BuscaCacheAsync(string hash, DateTime agora);
    Task RegistrarGeracaoAsync(IaCache cache, string? aplicacaoid, DateOnly? data, bool incrementarUso);
}
