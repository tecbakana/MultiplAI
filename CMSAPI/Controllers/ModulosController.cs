using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSXData.Models;
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

        if (!string.IsNullOrEmpty(usuarioid))
            return Ok(await _repo.ListaPorUsuarioAsync(usuarioid));

        if (acessoTotal)
            return Ok(await _repo.ListaTodosAsync());

        return Ok(await _repo.ListaPorAplicacaoAsync(claimAppId!));
    }
}
