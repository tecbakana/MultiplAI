using CMSAPIPublica.Dtos;
using ICMSX;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace CMSAPIPublica.Controllers;

[ApiController]
[Route("api/publico")]
[EnableRateLimiting("api_publica")]
public class SitePublicoController(
    ISiteRepositorio siteRepo,
    IPublicTokenRepositorio tokenRepo,
    IAplicacaoRepositorio aplicacaoRepo,
    IVitrineRepositorio vitrineRepo) : ControllerBase
{
    [HttpGet("site/resolve")]
    public async Task<IActionResult> Resolve([FromQuery] string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return BadRequest(new { message = "slug é obrigatório." });

        var app = await siteRepo.BuscaPorSlugAsync(slug);
        if (app == null)
            return NotFound(new { message = $"Site '{slug}' não encontrado." });

        var publicToken = await tokenRepo.BuscaPorAplicacaoAsync(app.Aplicacaoid!);
        if (publicToken == null)
            return NotFound(new { message = "Site sem token público ativo." });

        return Ok(new { token = publicToken.Token });
    }

    [HttpGet("{token}/site")]
    public async Task<IActionResult> GetSite(string token)
    {
        var aplicacaoid = await tokenRepo.ResolveAsync(token);
        if (aplicacaoid == null)
            return NotFound(new { message = "Token inválido ou expirado." });

        var app = await aplicacaoRepo.BuscaPorIdAsync(aplicacaoid);
        if (app == null)
            return NotFound(new { message = "Aplicação não encontrada." });

        var areas = await siteRepo.ListaAreasAsync(aplicacaoid);

        var areaIds = areas
            .Where(a => !string.IsNullOrEmpty(a.Areaid))
            .Select(a => a.Areaid!)
            .ToList();
        var snapshots = await vitrineRepo.BuscaSnapshotsPorAreasAsync(areaIds);

        var areasResult = new List<AreaPublicoResponse>();
        foreach (var area in areas)
        {
            var blocos = new List<BlocoPublicoResponse>();

            if (!string.IsNullOrEmpty(area.Layout) && area.Layout != "{\"blocos\":[]}")
            {
                try
                {
                    var doc = JsonDocument.Parse(area.Layout);
                    foreach (var b in doc.RootElement.GetProperty("blocos").EnumerateArray())
                    {
                        var tipo = b.GetProperty("tipo").GetString() ?? "";
                        var configRaw = b.GetProperty("config").GetRawText();
                        var config = JsonDocument.Parse(configRaw).RootElement;
                        var coluna = b.TryGetProperty("coluna", out var colunaEl) ? colunaEl.GetString() : null;
                        var dados = await EnriquecerAsync(tipo, config, aplicacaoid);
                        blocos.Add(new BlocoPublicoResponse(tipo, JsonSerializer.Deserialize<object>(configRaw), coluna, dados));
                    }
                }
                catch (JsonException)
                {
                    // Layout malformado: ignora blocos desta área
                }
            }

            var areaSnapshot = area.Areaid != null && snapshots.TryGetValue(area.Areaid, out var snap) ? snap : null;
            areasResult.Add(new AreaPublicoResponse(area.Areaid, area.Nome, area.Url, blocos.Count > 0, blocos, areaSnapshot));
        }

        return Ok(new SitePublicoResponse(app.Nome, app.Url, app.Header, areasResult));
    }

    [HttpGet("{token}/logo")]
    public async Task<IActionResult> GetLogo(string token)
    {
        var aplicacaoid = await tokenRepo.ResolveAsync(token);
        if (aplicacaoid == null) return NotFound();
        var (bytes, contentType) = await aplicacaoRepo.BuscaLogoAsync(aplicacaoid);
        if (bytes is null) return NotFound();
        Response.Headers.CacheControl = "public, max-age=3600";
        return File(bytes, contentType!);
    }

    private async Task<object?> EnriquecerAsync(string tipo, JsonElement config, string aplicacaoid)
    {
        switch (tipo)
        {
            case "lista-conteudos":
            {
                var areaid = Str(config, "areaid");
                if (string.IsNullOrEmpty(areaid)) return null;
                return (await siteRepo.ListaConteudosPorAreaAsync(areaid, Int(config, "limite", 6)))
                    .Select(c => new { c.Conteudoid, c.Titulo, c.Texto, c.Autor, c.Datainclusao })
                    .ToList();
            }
            case "lista-produtos":
            {
                var catid = Str(config, "cateriaid");
                return (await siteRepo.ListaProdutosAsync(aplicacaoid, catid, Int(config, "limite", 8)))
                    .Select(p => new { p.Produtoid, p.Nome, p.Descricacurta, p.Valor, p.Imagem })
                    .ToList();
            }
            case "categorias":
            {
                var pai = Str(config, "cateriaidpai");
                return (await siteRepo.ListaCategoriasAsync(aplicacaoid, pai))
                    .Select(c => new { c.Cateriaid, c.Nome, c.Descricao })
                    .ToList();
            }
            case "faq":
            {
                var formularioid = Str(config, "formularioid");
                if (string.IsNullOrEmpty(formularioid)) return null;
                return (await siteRepo.ListaFaqsAtivosAsync(formularioid))
                    .Select(f => new { f.Faqid, f.Pergunta, f.Resposta })
                    .ToList();
            }
            case "formulario":
            {
                var formularioid = Str(config, "formularioid");
                if (string.IsNullOrEmpty(formularioid)) return null;
                var f = await siteRepo.BuscaFormularioAsync(formularioid);
                return f == null ? null : new { f.Formularioid, f.Nome, f.Valor };
            }
            case "menu-navegacao":
            {
                return (await siteRepo.ListaAreasMenuAsync(aplicacaoid))
                    .Select(a => new { a.Areaid, a.Nome, a.Url })
                    .ToList();
            }
            case "prova-social":
            case "video":
            case "contador":
            case "hero-cta":
            case "rodape":
            default:
                return null;
        }
    }

    private static string? Str(JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) ? v.GetString() : null;

    private static int Int(JsonElement el, string key, int def) =>
        el.TryGetProperty(key, out var v) && v.TryGetInt32(out var n) ? n : def;
}
