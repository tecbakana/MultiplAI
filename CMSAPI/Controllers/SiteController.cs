using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ICMSX;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly ISiteRepositorio _repo;
        private readonly IAplicacaoRepositorio _aplicacaoRepo;
        private readonly IVitrineRepositorio _vitrineRepo;

        public SiteController(ISiteRepositorio repo, IAplicacaoRepositorio aplicacaoRepo, IVitrineRepositorio vitrineRepo)
        {
            _repo = repo;
            _aplicacaoRepo = aplicacaoRepo;
            _vitrineRepo = vitrineRepo;
        }

        [AllowAnonymous]
        [HttpGet("slug/{slug}")]
        public  async Task<IActionResult> GetBySlug(string slug)
        {
            var app = await _repo.BuscaPorSlugAsync(slug);
            return app == null
                ? NotFound(new { erro = $"Site '{slug}' não encontrado." })
                : await BuildSiteData(app.Aplicacaoid!, app.Nome, app.Url, app.Header);
        }

        [Authorize]
        [HttpGet("preview/{aplicacaoid}")]
        public  async Task<IActionResult> GetPreview(string aplicacaoid)
        {
            var app = await _aplicacaoRepo.BuscaPorIdAsync(aplicacaoid);
            return app == null ? NotFound() : await BuildSiteData(app.Aplicacaoid!, app.Nome, app.Url, app.Header);
        }

        private async Task<IActionResult> BuildSiteData(string aplicacaoid, string? nome, string? url, string? header)
        {
            var areas = await _repo.ListaAreasAsync(aplicacaoid);
            var snapshot = await _vitrineRepo.BuscaSnapshotAsync(aplicacaoid);

            var areasResult = new List<object>();
            foreach (var area in areas)
            {
                var blocos = new List<object>();
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
                            var dados = await EnriquecerBloco(tipo, config, aplicacaoid);
                            blocos.Add(new { tipo, config = JsonSerializer.Deserialize<object>(configRaw), coluna, dados });
                        }
                    }
                    catch (JsonException)
                    {
                        throw new Exception($"Erro ao processar layout da área '{area.Areaid}'. Verifique se o JSON está correto.");
                    }
                }
                var areaSnapshot = area.Tipo == "home" ? snapshot : null;
                areasResult.Add(new { area.Areaid, area.Nome, area.Url, TemLayout = blocos.Count > 0, blocos, HtmlSnapshot = areaSnapshot });
            }

            return Ok(new { Aplicacaoid = aplicacaoid, Nome = nome, Url = url, Header = header, areas = areasResult });
        }

        private async Task<object?> EnriquecerBloco(string tipo, JsonElement config, string aplicacaoid)
        {
            switch (tipo)
            {
                case "lista-conteudos":
                {
                    var areaid = Str(config, "areaid");
                    if (string.IsNullOrEmpty(areaid)) return null;
                    return (await _repo.ListaConteudosPorAreaAsync(areaid, Int(config, "limite", 6)))
                        .Select(c => new { c.Conteudoid, c.Titulo, c.Texto, c.Autor, c.Datainclusao })
                        .ToList();
                }
                case "lista-produtos":
                {
                    var catid = Str(config, "cateriaid");
                    return (await _repo.ListaProdutosAsync(aplicacaoid, catid, Int(config, "limite", 8)))
                        .Select(p => new { p.Produtoid, p.Nome, p.Descricacurta, p.Valor, p.Imagem })
                        .ToList();
                }
                case "categorias":
                {
                    var pai = Str(config, "cateriaidpai");
                    return (await _repo.ListaCategoriasAsync(aplicacaoid, pai))
                        .Select(c => new { c.Cateriaid, c.Nome, c.Descricao })
                        .ToList();
                }
                case "faq":
                {
                    var formularioid = Str(config, "formularioid");
                    if (string.IsNullOrEmpty(formularioid)) return null;
                    return (await _repo.ListaFaqsAtivosAsync(formularioid))
                        .Select(f => new { f.Faqid, f.Pergunta, f.Resposta })
                        .ToList();
                }
                case "formulario":
                {
                    var formularioid = Str(config, "formularioid");
                    if (string.IsNullOrEmpty(formularioid)) return null;
                    var f = await _repo.BuscaFormularioAsync(formularioid);
                    return f == null ? null : new { f.Formularioid, f.Nome, f.Valor };
                }
                case "menu-navegacao":
                {
                    return (await _repo.ListaAreasMenuAsync(aplicacaoid))
                        .Select(a => new { a.Areaid, a.Nome, a.Url })
                        .ToList();
                }
                case "prova-social":
                case "video":
                case "contador":
                case "hero-cta":
                case "rodape":
                    return null;
                default: return null;
            }
        }

        private static string? Str(JsonElement el, string key) =>
            el.TryGetProperty(key, out var v) ? v.GetString() : null;

        private static int Int(JsonElement el, string key, int def) =>
            el.TryGetProperty(key, out var v) && v.TryGetInt32(out var n) ? n : def;
    }
}
