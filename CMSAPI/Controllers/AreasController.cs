using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AreasController : Controller
{
    private readonly IAreasRepositorio _repo;
    public AreasController(IAreasRepositorio repo) { _repo = repo; }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? aplicacaoid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var filtro = acessoTotal ? aplicacaoid : claimAppId;
        return Ok(await _repo.ListaAsync(filtro));
    }

    public class NovaAreaDto
    {
        public string? Nome { get; set; }
        public string? Url { get; set; }
        public string? Descricao { get; set; }
        public string? Areaidpai { get; set; }
        public int? Posicao { get; set; }
        public int? Tipoarea { get; set; }
        public string? Aplicacaoid { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] NovaAreaDto dto)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var item = new Area
        {
            Areaid      = Guid.NewGuid().ToString(),
            Nome        = dto.Nome,
            Url         = dto.Url,
            Descricao   = dto.Descricao,
            Areaidpai   = dto.Areaidpai,
            Posicao     = dto.Posicao,
            Tipoarea    = dto.Tipoarea,
            Aplicacaoid = acessoTotal ? dto.Aplicacaoid : claimAppId,
            Datainicial = DateTime.UtcNow
        };
        await _repo.CriarAsync(item);
        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] NovaAreaDto dto)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var item = await _repo.BuscaPorIdAsync(id);
        if (item == null) return NotFound();
        if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
        item.Nome      = dto.Nome;
        item.Url       = dto.Url;
        item.Descricao = dto.Descricao;
        item.Areaidpai = dto.Areaidpai;
        item.Posicao   = dto.Posicao;
        item.Tipoarea  = dto.Tipoarea;
        await _repo.AtualizarAsync(item);
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var item = await _repo.BuscaPorIdAsync(id);
        if (item == null) return NotFound();
        if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
        await _repo.RemoverAsync(item);
        return Ok();
    }
}
