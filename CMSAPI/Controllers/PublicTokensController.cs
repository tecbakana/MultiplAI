using CMSXData.Models;
using ICMSX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CMSAPI.Controllers;

[ApiController]
[Route("publictokens")]
[Authorize]
public class PublicTokensController : Controller
{
    private readonly IPublicTokenRepositorio _repo;

    public PublicTokensController(IPublicTokenRepositorio repo)
    {
        _repo = repo;
    }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True",
         User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public  async Task<IActionResult> Get([FromQuery] string? aplicacaoid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = acessoTotal && !string.IsNullOrEmpty(aplicacaoid) ? aplicacaoid : claimAppId;

        if (string.IsNullOrEmpty(appId)) return Forbid();

        var lista = (await _repo.ListaAsync(appId)).Select(t => new
        {
            t.PublicTokenId,
            t.Token,
            t.Ativo,
            t.Datainclusao,
            t.Datavencimento
        });

        return Ok(lista);
    }

    public class GerarTokenDto
    {
        public string? Aplicacaoid { get; set; }
        public DateTime? Datavencimento { get; set; }
    }

    [HttpPost]
    public  async Task<IActionResult> Gerar([FromBody] GerarTokenDto dto)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = acessoTotal && !string.IsNullOrEmpty(dto?.Aplicacaoid) ? dto.Aplicacaoid : claimAppId;

        if (string.IsNullOrEmpty(appId)) return Forbid();

        var token = new PublicToken
        {
            PublicTokenId  = Guid.NewGuid(),
            Token          = GerarTokenAleatorio(),
            Aplicacaoid    = appId,
            Ativo          = true,
            Datainclusao   = DateTime.UtcNow,
            Datavencimento = dto?.Datavencimento
        };

        await _repo.CriarAsync(token);

        return Ok(new { token.PublicTokenId, token.Token, token.Datainclusao });
    }

    [HttpDelete("{id}")]
    public  async Task<IActionResult> Revogar(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();

        var token = await _repo.BuscaPorIdAsync(id);
        if (token == null) return NotFound();
        if (!acessoTotal && token.Aplicacaoid != claimAppId) return Forbid();

        await _repo.RevogarAsync(token);
        return Ok();
    }

    private static string GerarTokenAleatorio()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
