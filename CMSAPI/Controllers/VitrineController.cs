using CMSAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using ICMSX;
using Microsoft.AspNetCore.Hosting;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class VitrineController : Controller
{
    private readonly IVitrineRepositorio _repo;
    private readonly IAreasRepositorio _areasRepo;
    private readonly ISiteRepositorio _siteRepo;
    private readonly IAplicacaoRepositorio _aplicacaoRepo;
    private readonly IVitrineGeracaoService _geracaoService;
    private readonly IWebHostEnvironment _env;

    public VitrineController(IVitrineRepositorio repo, IAreasRepositorio areasRepo,
        ISiteRepositorio siteRepo, IAplicacaoRepositorio aplicacaoRepo,
        IVitrineGeracaoService geracaoService, IWebHostEnvironment env)
    {
        _repo = repo;
        _areasRepo = areasRepo;
        _siteRepo = siteRepo;
        _aplicacaoRepo = aplicacaoRepo;
        _geracaoService = geracaoService;
        _env = env;
    }

    private async Task<string?> LerDesignSystemCssAsync()
    {
        var path = Path.Combine(_env.WebRootPath, "vitrine", "design-system.css");
        return System.IO.File.Exists(path) ? await System.IO.File.ReadAllTextAsync(path) : null;
    }

    private string? AplicacaoId() => User.FindFirstValue("aplicacaoid");

    private async Task<string?> BuildNavHtmlAsync(string appId)
    {
        var app = await _aplicacaoRepo.BuscaPorIdAsync(appId);
        if (app?.Url is null) return null;

        var (logoBytes, logoContentType) = await _aplicacaoRepo.BuscaLogoAsync(appId);
        var logoHtml = logoBytes is { Length: > 0 } && logoContentType is not null
            ? $"<img class=\"v-nav__logo\" src=\"data:{logoContentType};base64,{Convert.ToBase64String(logoBytes)}\" alt=\"Logo\" />"
            : "";

        var areas = await _siteRepo.ListaAreasMenuAsync(appId);
        var links = string.Concat(areas
            .Where(a => !string.IsNullOrEmpty(a.Url) && !string.IsNullOrEmpty(a.Nome))
            .Select(a => $"<a href=\"/s/{app.Url}/{WebUtility.HtmlEncode(a.Url)}\">{WebUtility.HtmlEncode(a.Nome)}</a>"));

        return logoHtml + links;
    }

    [HttpGet("templates")]
    public async Task<IActionResult> ListaTemplates()
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var templates = await _repo.ListaTemplatesDisponiveisAsync(appId);
        return Ok(templates);
    }

    [HttpGet("templates/{id:guid}")]
    public async Task<IActionResult> BuscaTemplate(Guid id)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var template = await _repo.BuscaTemplateAsync(id);
        if (template is null) return NotFound();
        var acessiveis = await _repo.ListaTemplatesDisponiveisAsync(appId);
        if (acessiveis.All(t => t.VitrineTemplateId != id)) return Forbid();
        return Ok(template);
    }

    [HttpGet("area/{areaId}/configurada")]
    public async Task<IActionResult> BuscaAreaConfigurada(string areaId)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var area = await _areasRepo.BuscaPorIdAsync(areaId);
        if (area is null) return NotFound();
        if (area.Aplicacaoid != appId) return Forbid();
        var config = await _repo.BuscaAreaConfigAsync(areaId);
        return config is null ? NotFound() : Ok(config);
    }

    [HttpPut("area/{areaId}/configurada")]
    public async Task<IActionResult> SalvarAreaConfigurada(string areaId, [FromBody] VitrineAreaConfigInput input)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var area = await _areasRepo.BuscaPorIdAsync(areaId);
        if (area is null) return NotFound();
        if (area.Aplicacaoid != appId) return Forbid();
        await _repo.SalvarAreaConfigAsync(areaId, input);
        return Ok();
    }

    [HttpPost("area/{areaId}/publicar")]
    public async Task<IActionResult> PublicarArea(string areaId)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var area = await _areasRepo.BuscaPorIdAsync(areaId);
        if (area is null) return NotFound();
        if (area.Aplicacaoid != appId) return Forbid();
        var temSchema = await _repo.BuscaAreaSchemaAsync(areaId) is not null;
        var cssContent = temSchema ? await LerDesignSystemCssAsync() : null;
        var htmlRendered = temSchema
            ? await _repo.RenderAreaSchemaAsync(areaId, _siteRepo, cssContent)
            : await _repo.RenderAreaAsync(areaId, _siteRepo);
        if (htmlRendered is null) return UnprocessableEntity(new { message = "Vitrine não configurada para esta área." });
        await _repo.PublicarAreaAsync(areaId, htmlRendered);
        return Ok();
    }

    [HttpPost("area/{areaId}/gerar")]
    public async Task<IActionResult> GerarArea(string areaId, [FromBody] VitrineGerarAreaInput input)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var area = await _areasRepo.BuscaPorIdAsync(areaId);
        if (area is null) return NotFound();
        if (area.Aplicacaoid != appId) return Forbid();

        var resultado = await _geracaoService.GerarAsync(areaId, appId, input);
        if (resultado.ErroAgente is not null)
            return StatusCode(502, new { message = "Erro ao chamar o agente de IA.", detalhe = resultado.ErroAgente });
        if (resultado.ErroJson is not null)
            return UnprocessableEntity(new { message = "IA retornou JSON inválido.", detalhe = resultado.ErroJson, raw = resultado.JsonBruto });
        if (resultado.ErrosValidacao is not null)
            return UnprocessableEntity(new { message = "Configuração fora do contrato.", erros = resultado.ErrosValidacao });
        return Ok(new { config = resultado.Config });
    }

    [HttpGet("area/{areaId}/render")]
    public async Task<IActionResult> RenderArea(string areaId)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var area = await _areasRepo.BuscaPorIdAsync(areaId);
        if (area is null) return NotFound();
        if (area.Aplicacaoid != appId) return Forbid();

        var temSchema = await _repo.BuscaAreaSchemaAsync(areaId) is not null;
        var cssContent = temSchema ? await LerDesignSystemCssAsync() : null;
        var html = temSchema
            ? await _repo.RenderAreaSchemaAsync(areaId, _siteRepo, cssContent)
            : await _repo.RenderAreaAsync(areaId, _siteRepo);

        if (html is null) return NotFound();
        return Content(html, "text/html");
    }

    [HttpGet("configurada")]
    public async Task<IActionResult> BuscaConfigurada()
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var configurada = await _repo.BuscaConfiguradaAsync(appId);
        return Ok(configurada);
    }

    [HttpPut("configurada")]
    public async Task<IActionResult> SalvarConfigurada([FromBody] VitrineConfiguradaInput input)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var id = await _repo.SalvarConfiguradaAsync(appId, input);
        return Ok(new { vitrineConfiguradaId = id });
    }

    [HttpPost("publicar")]
    public async Task<IActionResult> Publicar()
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();

        var navHtml = await BuildNavHtmlAsync(appId);
        var html = await _repo.RenderComDadosAsync(appId, _siteRepo, navHtml: navHtml);
        if (html is null) return BadRequest(new { message = "Nenhuma vitrine configurada." });

        var ok = await _repo.PublicarAsync(appId, html);
        return ok ? Ok() : NotFound();
    }

    [HttpGet("render")]
    public async Task<IActionResult> Render([FromQuery] string? areaId = null)
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();

        if (areaId is null)
        {
            var snapshot = await _repo.BuscaSnapshotAsync(appId);
            if (snapshot is not null)
                return Content(snapshot, "text/html");
        }

        string? conteudoPb = null;
        if (!string.IsNullOrEmpty(areaId))
        {
            var area = await _areasRepo.BuscaPorIdAsync(areaId);
            if (area is null) return NotFound();
            if (area.Aplicacaoid != appId) return Forbid();
            conteudoPb = area.Layout;
        }

        var navHtml = await BuildNavHtmlAsync(appId);
        var html = await _repo.RenderComDadosAsync(appId, _siteRepo, navHtml: navHtml, conteudoPb: conteudoPb);
        if (html is null) return NotFound();
        return Content(html, "text/html");
    }

    [HttpGet("css/{vitrineConfiguradaId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Css(Guid vitrineConfiguradaId)
    {
        var css = await _repo.BuscaCssProcessadoAsync(vitrineConfiguradaId);
        if (css is null) return NotFound();
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        return Content(css, "text/css");
    }
}
