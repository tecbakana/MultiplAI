using System.Text.Json;
using ICMSX;

namespace CMSAPI.Services;

public record VitrineGeracaoResult(
    VitrineConfig? Config = null,
    string? ErroAgente = null,
    string? ErroJson = null,
    string? JsonBruto = null,
    IReadOnlyList<string>? ErrosValidacao = null);

public interface IVitrineGeracaoService
{
    Task<VitrineGeracaoResult> GerarAsync(string areaId, string appId, VitrineGerarAreaInput input);
}

public class VitrineGeracaoService : IVitrineGeracaoService
{
    private readonly IVitrineRepositorio _repo;
    private readonly IAplicacaoRepositorio _aplicacaoRepo;
    private readonly ISegmentoTenantRepositorio _segmentoRepo;
    private readonly IAgentIAFactory _agentFactory;

    public VitrineGeracaoService(
        IVitrineRepositorio repo,
        IAplicacaoRepositorio aplicacaoRepo,
        ISegmentoTenantRepositorio segmentoRepo,
        IAgentIAFactory agentFactory)
    {
        _repo = repo;
        _aplicacaoRepo = aplicacaoRepo;
        _segmentoRepo = segmentoRepo;
        _agentFactory = agentFactory;
    }

    public async Task<VitrineGeracaoResult> GerarAsync(string areaId, string appId, VitrineGerarAreaInput input)
    {
        var app = await _aplicacaoRepo.BuscaPorIdAsync(appId);

        var segmento = input.Segmento;
        if (string.IsNullOrEmpty(segmento))
        {
            var segmentos = await _segmentoRepo.ListaPorAplicacaoAsync(appId);
            segmento = segmentos.FirstOrDefault()?.Nome ?? "geral";
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Nome: {app?.Nome ?? "Site"}");
        sb.AppendLine($"Segmento: {segmento}");
        if (!string.IsNullOrEmpty(input.Tipo))
            sb.AppendLine($"Tipo de página: {input.Tipo}");
        if (!string.IsNullOrEmpty(input.Estilo))
            sb.AppendLine($"Estilo visual: {input.Estilo}");
        if (!string.IsNullOrEmpty(input.Paleta))
            sb.AppendLine($"Paleta de cores: {input.Paleta}");
        if (!string.IsNullOrEmpty(input.TemaCanonicoJson))
            sb.AppendLine($"Tema canônico (preservar estas propriedades de cores e tipografia): {input.TemaCanonicoJson}");
        sb.AppendLine($"Pedido: {input.Prompt}");
        var contexto = sb.ToString();

        string jsonBruto;
        try
        {
            var agente = _agentFactory.Criar(input.Provedor);
            jsonBruto = await agente.GerarComSistemaAsync(VitrineSystemPrompt.Texto, contexto);
        }
        catch (Exception ex)
        {
            return new VitrineGeracaoResult(ErroAgente: ex.Message);
        }

        VitrineConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<VitrineConfig>(jsonBruto, VitrineJsonOptions.Padrao);
        }
        catch (JsonException ex)
        {
            return new VitrineGeracaoResult(ErroJson: ex.Message, JsonBruto: jsonBruto);
        }

        if (config is null)
            return new VitrineGeracaoResult(ErroJson: "IA retornou resposta nula.", JsonBruto: jsonBruto);

        var erros = VitrineConfigValidator.Validar(config);
        if (erros.Count > 0)
            return new VitrineGeracaoResult(ErrosValidacao: erros);

        await _repo.SalvarAreaSchemaAsync(areaId, config);
        return new VitrineGeracaoResult(Config: config);
    }
}
