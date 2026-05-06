using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ConteudosController : Controller
{
    private readonly IConteudoRepositorio _repo;
    public ConteudosController(IConteudoRepositorio repo) { _repo = repo; }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? areaid = null, [FromQuery] string? cateriaid = null, [FromQuery] string? aplicacaoid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var filtroApp = acessoTotal ? aplicacaoid : claimAppId;
        return Ok(await _repo.ListaAsync(filtroApp, areaid, cateriaid));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var item = await _repo.BuscaPorIdAsync(id);
        if (item == null) return NotFound();
        if (!acessoTotal)
        {
            var appId = await _repo.AplicacaoidDaAreaAsync(item.Areaid);
            if (appId != claimAppId) return Forbid();
        }
        return Ok(item);
    }

    public class NovoConteudoDto
    {
        public string? Titulo { get; set; }
        public string? Texto { get; set; }
        public string? Autor { get; set; }
        public string? Areaid { get; set; }
        public string? Cateriaid { get; set; }
        public DateTime? Datafinal { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] NovoConteudoDto dto)
    {
        var (acessoTotal, claimAppId) = UserContext();
        if (!acessoTotal && !string.IsNullOrEmpty(dto.Areaid))
        {
            var appId = await _repo.AplicacaoidDaAreaAsync(dto.Areaid);
            if (appId != claimAppId) return Forbid();
        }

        var item = new Conteudo
        {
            Conteudoid   = Guid.NewGuid().ToString(),
            Titulo       = dto.Titulo,
            Texto        = dto.Texto,
            Autor        = dto.Autor,
            Areaid       = dto.Areaid,
            Cateriaid    = dto.Cateriaid,
            Datafinal    = dto.Datafinal,
            Datainclusao = DateTime.UtcNow
        };
        await _repo.CriarAsync(item);
        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Conteudo item)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var existing = await _repo.BuscaPorIdAsync(id);
        if (existing == null) return NotFound();
        if (!acessoTotal)
        {
            var appId = await _repo.AplicacaoidDaAreaAsync(existing.Areaid);
            if (appId != claimAppId) return Forbid();
        }
        existing.Titulo    = item.Titulo;
        existing.Texto     = item.Texto;
        existing.Autor     = item.Autor;
        existing.Cateriaid = item.Cateriaid;
        existing.Areaid    = item.Areaid;
        existing.Datafinal = item.Datafinal;
        await _repo.AtualizarAsync(existing);
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var item = await _repo.BuscaPorIdAsync(id);
        if (item == null) return NotFound();
        if (!acessoTotal)
        {
            var appId = await _repo.AplicacaoidDaAreaAsync(item.Areaid);
            if (appId != claimAppId) return Forbid();
        }
        await _repo.RemoverAsync(item);
        return Ok();
    }
}
