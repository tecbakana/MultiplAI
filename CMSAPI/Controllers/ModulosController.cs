using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ICMSX;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ModulosController : Controller
{
    private readonly IModuloRepositorio _repo;
    public ModulosController(IModuloRepositorio repo) { _repo = repo; }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? usuarioid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();

        // usuarioid externo: exclusivo para admin — não-admin não pode enumerar outro usuário
        if (!string.IsNullOrEmpty(usuarioid))
        {
            if (!acessoTotal) return Forbid();
            return Ok(await _repo.ListaPorUsuarioAsync(usuarioid));
        }

        if (acessoTotal)
            return Ok(await _repo.ListaTodosAsync());

        // não-admin: userid vem do JWT, nunca do cliente
        var claimUserId = User.FindFirstValue("userid");
        return Ok(await _repo.ListaPorUsuarioAsync(claimUserId!));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ModuloInput input)
    {
        var (acessoTotal, _) = UserContext();
        if (!acessoTotal) return Forbid();

        var moduloid = await _repo.CriarAsync(input);
        return Ok(new { moduloid });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] ModuloInput input)
    {
        var (acessoTotal, _) = UserContext();
        if (!acessoTotal) return Forbid();

        var atualizado = await _repo.AtualizarAsync(id, input);
        if (!atualizado) return NotFound();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (acessoTotal, _) = UserContext();
        if (!acessoTotal) return Forbid();

        var removido = await _repo.RemoverAsync(id);
        if (!removido) return NotFound();
        return NoContent();
    }
}
