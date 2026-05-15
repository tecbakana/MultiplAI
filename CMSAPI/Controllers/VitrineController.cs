using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using ICMSX;

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

    public VitrineController(IVitrineRepositorio repo, IAreasRepositorio areasRepo,
        ISiteRepositorio siteRepo, IAplicacaoRepositorio aplicacaoRepo)
    {
        _repo = repo;
        _areasRepo = areasRepo;
        _siteRepo = siteRepo;
        _aplicacaoRepo = aplicacaoRepo;
    }

    private string? AplicacaoId() => User.FindFirstValue("aplicacaoid");

    private async Task<string?> BuildNavHtmlAsync(string appId)
    {
        var app = await _aplicacaoRepo.BuscaPorIdAsync(appId);
        if (app?.Url is null) return null;

        var areas = await _siteRepo.ListaAreasMenuAsync(appId);
        return string.Concat(areas
            .Where(a => !string.IsNullOrEmpty(a.Url) && !string.IsNullOrEmpty(a.Nome))
            .Select(a => $"<a href=\"/s/{app.Url}/{System.Net.WebUtility.HtmlEncode(a.Url)}\">{System.Net.WebUtility.HtmlEncode(a.Nome)}</a>"));
    }

    [HttpGet("templates")]
    public async Task<IActionResult> ListaTemplates()
    {
        var appId = AplicacaoId();
        if (string.IsNullOrEmpty(appId)) return Forbid();
        var templates = await _repo.ListaTemplatesDisponiveisAsync(appId);
        return Ok(templates);
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
