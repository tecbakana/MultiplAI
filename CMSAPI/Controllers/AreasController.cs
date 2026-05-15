using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

    [HttpGet("{aplicacaoId}/home")]
    public async Task<IActionResult> GetHome(string aplicacaoId)
    {
        var (acessoTotal, claimAppId) = UserContext();
        if (!acessoTotal && claimAppId != aplicacaoId) return Forbid();
        return Ok(await _repo.BuscaHomeAsync(aplicacaoId));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AreaInput input, [FromQuery] string? aplicacaoid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = acessoTotal ? aplicacaoid : claimAppId;

        if (input.Tipo == "home" && appId != null && await _repo.ExisteHomeAsync(appId))
            return Conflict("Já existe uma área home para esta aplicação.");

        var id = await _repo.CriarAsync(input, appId!);
        return Ok(new { areaid = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] AreaInput dto)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var item = await _repo.BuscaPorIdAsync(id);
        if (item == null) return NotFound();
        if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();

        if (dto.Tipo == "home" && item.Aplicacaoid != null &&
            await _repo.ExisteHomeAsync(item.Aplicacaoid, excluirAreaId: id))
            return Conflict("Já existe uma área home para esta aplicação.");

        var atualizado = await _repo.AtualizarAsync(id, dto);
        if (!atualizado) return NotFound();
        return Ok();
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
