using CMSAPI.Services;
using ICMSX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SegmentosController : Controller
{
    private readonly ISegmentoTenantRepositorio _repo;
    private readonly IProdutoTemplateRepositorio _templateRepo;
    private readonly IAgentIAFactory _agentFactory;

    public SegmentosController(
        ISegmentoTenantRepositorio repo,
        IProdutoTemplateRepositorio templateRepo,
        IAgentIAFactory agentFactory)
    {
        _repo        = repo;
        _templateRepo = templateRepo;
        _agentFactory = agentFactory;
    }

    private bool IsAdmin() => User.FindFirstValue("acessoTotal") == "True";
    private string? AplicacaoId() => User.FindFirstValue("aplicacaoid");
    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));


    // ── CRUD Segmentos (admin) ────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (!IsAdmin()) return Forbid();
        return Ok(await _repo.ListaAtivosAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!IsAdmin()) return Forbid();
        var s = await _repo.BuscaPorIdAsync(id);
        return s == null ? NotFound() : Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] SegmentoTenantInput input)
    {
        if (!IsAdmin()) return Forbid();
        var id = await _repo.CriarAsync(input);
        return Ok(new { segmentoTenantId = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] SegmentoTenantInput input)
    {
        if (!IsAdmin()) return Forbid();
        var atualizado = await _repo.AtualizarAsync(id, input);
        if (!atualizado) return NotFound();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!IsAdmin()) return Forbid();
        var removido = await _repo.RemoverAsync(id);
        if (!removido) return NotFound();
        return NoContent();
    }

    // ── Templates por segmento (admin) ────────────────────────────────────────

    [HttpGet("{id}/templates")]
    public async Task<IActionResult> GetTemplates(string id)
    {
        if (!IsAdmin()) return Forbid();
        var segmento = await _repo.BuscaPorIdAsync(id);
        if (segmento == null) return NotFound();
        return Ok(await _templateRepo.ListaPorSegmentoAsync(id));
    }

    // ── Geração de templates via IA ───────────────────────────────────────────

    [HttpPost("{id}/templates/gerar")]
    public async Task<IActionResult> GerarTemplates(string id, [FromBody] GerarSegmentoTemplatesInput dto)
    {
        if (!IsAdmin()) return Forbid();

        var segmento = await _repo.BuscaPorIdAsync(id);
        if (segmento == null) return NotFound();

        const int totalTemplates = 3;
        var agente = _agentFactory.Criar();
        var ids = new List<string>();
        var erros = new List<string>();

        for (var i = 1; i <= totalTemplates; i++)
        {
            string raw;
            try
            {
                raw = LimparJsonObjeto(await agente.GerarAsync(MontarPromptSegmento(segmento.Nome, dto.PromptSegmento, i, totalTemplates)));
            }
            catch (HttpRequestException ex)
            {
                erros.Add($"Template {i}: erro ao chamar IA — {ex.Message}");
                continue;
            }
            catch (InvalidOperationException ex)
            {
                erros.Add($"Template {i}: {ex.Message}");
                continue;
            }

            JsonElement elemento;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                elemento = doc.RootElement.Clone();

                if (elemento.ValueKind != JsonValueKind.Object)
                {
                    erros.Add($"Template {i}: IA não retornou objeto válido.");
                    continue;
                }
            }
            catch (JsonException ex)
            {
                erros.Add($"Template {i}: JSON inválido — {ex.Message}");
                continue;
            }

            var nome = elemento.TryGetProperty("nome", out var n) ? n.GetString() ?? "Template" : "Template";
            var descricao = elemento.TryGetProperty("descricao", out var d) ? d.GetString() : null;
            var tid = await _templateRepo.CriarSegmentoAsync(
                new ProdutoTemplateInput(nome, descricao, elemento.GetRawText()), id);
            ids.Add(tid);
        }

        return Ok(new { gerados = ids.Count, ids, erros });
    }

    // ── Vínculos Aplicação ↔ Segmento (admin) ────────────────────────────────

    [HttpGet("~/Admin/Aplicacoes/segmentos")]
    public async Task<IActionResult> GetSegmentosPorAplicacao()
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
        if (!IsAdmin()) return Forbid();
        return Ok(await _repo.ListaPorAplicacaoAsync(appId));
    }

    [HttpPost("~/Admin/Aplicacoes/segmentos/{segmentoId}")]
    public async Task<IActionResult> VincularAdmin( string segmentoId)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
        if (!IsAdmin()) return Forbid();
        if (await _repo.BuscaPorIdAsync(segmentoId) == null) return NotFound();
        await _repo.VincularAsync(appId, segmentoId);
        return Ok();
    }

    [HttpDelete("~/Admin/Aplicacoes/segmentos/{segmentoId}")]
    public async Task<IActionResult> DesvincularAdmin(string segmentoId)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
        if (!IsAdmin()) return Forbid();
        await _repo.DesvincularAsync(appId, segmentoId);
        return NoContent();
    }

    // ── Segmentos disponíveis e vínculos do próprio tenant ───────────────────

    [HttpGet("~/Segmentos/disponiveis")]
    public async Task<IActionResult> GetDisponiveis() =>
        Ok(await _repo.ListaAtivosAsync());

    [HttpGet("~/Segmentos/minha-aplicacao")]
    public async Task<IActionResult> GetMinhaAplicacao()
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
        return Ok(await _repo.ListaPorAplicacaoAsync(appId));
    }

    [HttpPost("~/Segmentos/minha-aplicacao/{segmentoId}")]
    public async Task<IActionResult> VincularMeu(string segmentoId)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
        if (await _repo.BuscaPorIdAsync(segmentoId) == null) return NotFound();
        await _repo.VincularAsync(appId, segmentoId);
        return Ok();
    }

    [HttpDelete("~/Segmentos/minha-aplicacao/{segmentoId}")]
    public async Task<IActionResult> DesvincularMeu(string segmentoId)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
        await _repo.DesvincularAsync(appId, segmentoId);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string MontarPromptSegmento(string nomeSegmento, string promptUsuario, int indice, int total) => $$"""
        Você é um especialista em catálogo de produtos para o segmento "{{nomeSegmento}}".
        Gere EXATAMENTE 1 template de produto em JSON (template {{indice}} de {{total}}).
        Retorne APENAS o objeto JSON, sem array, sem markdown, sem blocos de código, sem texto adicional.

        INSTRUÇÃO:
        {{promptUsuario}}

        ESTRUTURA DO OBJETO:
        {
          "nome": "nome do produto",
          "descricao": "descrição completa",
          "descricaoCurta": "resumo em até 100 caracteres",
          "detalheTecnico": "especificações técnicas relevantes",
          "valor": 0.00,
          "unidadeVenda": "UN",
          "atributos": [
            {
              "nome": "nome do atributo",
              "descricao": "descrição",
              "ordem": 1,
              "valorAdicional": 0.00,
              "opcoes": ["opção 1", "opção 2"],
              "filhos": []
            }
          ]
        }

        REGRAS:
        - Retorne apenas 1 objeto JSON completo, sem array externo
        - O template deve ser diferente dos demais da sequência (este é o {{indice}}º de {{total}})
        - Valores monetários são decimais (ex: 10.50); use 0.00 quando desconhecido
        - JSON ESTRITO obrigatório: todas as chaves e valores string com aspas duplas ("nome", não nome)
        - Proibido markdown, blocos de código, comentários ou qualquer texto fora do objeto JSON
        - Retorne APENAS o objeto JSON, absolutamente nada mais
        """;

    private static string LimparJsonObjeto(string texto)
    {
        var inicio = texto.IndexOf('{');
        var fim = texto.LastIndexOf('}');
        if (inicio >= 0 && fim > inicio)
            return texto[inicio..(fim + 1)];
        return texto.Trim();
    }
}

