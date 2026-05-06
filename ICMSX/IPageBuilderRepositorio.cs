using CMSXData.Models;

namespace ICMSX;

public interface IPageBuilderRepositorio
{
    IEnumerable<DictBloco> ListaBlocos();
    IEnumerable<string> ListaTiposBlocos();
    IaConfig? BuscaConfig(string aplicacaoid);
    void SalvarConfig(string aplicacaoid, string? provedor, string? modelo, int? limiteDiario, string? apikey);
    void RemoverApiKey(string aplicacaoid);
    int BuscaUsoHoje(string aplicacaoid, DateOnly data);
    IaCache? BuscaCache(string hash, DateTime agora);
    void RegistrarGeracao(IaCache cache, string? aplicacaoid, DateOnly? data, bool incrementarUso);
}
