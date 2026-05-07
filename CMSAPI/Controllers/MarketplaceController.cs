using CMSAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MarketplaceController(
    IMarketHubHttpService marketHub,
    IConfiguration config,
    IHttpClientFactory httpClientFactory) : Controller
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
    private string? AplicacaoId => User.FindFirstValue("aplicacaoid");

    [HttpGet("configuracoes")]
    public async Task<IActionResult> GetConfiguracoes()
    {
        if (AplicacaoId == null) return Forbid();

        var json = await marketHub.GetConfiguracoes(AplicacaoId);
        if (json == null) return StatusCode(502, new { erro = "Erro ao comunicar com MarketHub" });

        return Content(json, "application/json");
    }

    [HttpPost("configuracoes")]
    public async Task<IActionResult> PostConfiguracao()
    {
        if (AplicacaoId == null) return Forbid();

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var sucesso = await marketHub.PostConfiguracao(AplicacaoId, body);
        if (!sucesso) return StatusCode(502, new { erro = "Erro ao comunicar com MarketHub" });

        return Ok(new { sucesso = true });
    }

    [HttpGet("pedidos")]
    public async Task<IActionResult> GetPedidos()
    {
        if (AplicacaoId == null) return Forbid();

        var json = await marketHub.GetPedidos(AplicacaoId);
        if (json == null) return StatusCode(502, new { erro = "Erro ao comunicar com MarketHub" });

        return Content(json, "application/json");
    }

    [HttpGet("pedidos/{id}")]
    public async Task<IActionResult> GetPedido(string id)
    {
        if (AplicacaoId == null) return Forbid();

        var (encontrado, json) = await marketHub.GetPedido(AplicacaoId, id);
        if (!encontrado) return NotFound();

        return Content(json!, "application/json");
    }

    [HttpPost("pedidos/{id}/emitir-nf")]
    public async Task<IActionResult> EmitirNf(string id)
    {
        if (AplicacaoId == null) return Forbid();

        var json = await marketHub.EmitirNf(AplicacaoId, id);
        if (json == null) return StatusCode(502, new { erro = "Erro ao comunicar com MarketHub" });

        return Content(json, "application/json");
    }

    [HttpDelete("configuracoes/{marketplace}")]
    public async Task<IActionResult> DeleteConfiguracao(string marketplace)
    {
        if (AplicacaoId == null) return Forbid();

        var sucesso = await marketHub.DeleteConfiguracao(AplicacaoId, marketplace);
        if (!sucesso) return StatusCode(502, new { erro = "Erro ao comunicar com MarketHub" });
        return Ok(new { sucesso = true });
    }

    [HttpGet("ml/connect")]
    [AllowAnonymous]
    public IActionResult MlConnect([FromQuery] string aplicacaoid)
    {
        if (string.IsNullOrWhiteSpace(aplicacaoid)) return BadRequest();

        var clientId = config["ML:ClientId"]!;
        var callbackUrl = Uri.EscapeDataString(config["ML:CallbackUrl"]!);

        var url = $"https://auth.mercadolivre.com.br/authorization?response_type=code&client_id={clientId}&redirect_uri={callbackUrl}&state={aplicacaoid}";
        return Redirect(url);
    }

    [HttpGet("ml/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> MlCallback([FromQuery] string code, [FromQuery] string state)
    {
        var postAuthUrl = config["ML:PostAuthRedirectUrl"]!;

        try
        {
            var clientId = config["ML:ClientId"]!;
            var clientSecret = config["ML:ClientSecret"]!;
            var callbackUrl = config["ML:CallbackUrl"]!;

            var http = httpClientFactory.CreateClient();
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code"] = code,
                ["redirect_uri"] = callbackUrl,
            });

            var tokenRes = await http.PostAsync("https://api.mercadolibre.com/oauth/token", form);
            if (!tokenRes.IsSuccessStatusCode)
                return Redirect($"{postAuthUrl}?ml_error=1");

            var body = await tokenRes.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body, _json);
            var accessToken = result.GetProperty("access_token").GetString()!;
            var refreshToken = result.GetProperty("refresh_token").GetString()!;
            var sellerId = result.TryGetProperty("user_id", out var uid) ? uid.GetInt64().ToString() : "";

            var payload = JsonSerializer.Serialize(new { marketplace = "mercadolivre", accessToken, refreshToken, sellerId });
            await marketHub.PostConfiguracao(state, payload);

            return Redirect($"{postAuthUrl}?ml_connected=1");
        }
        catch
        {
            return Redirect($"{postAuthUrl}?ml_error=1");
        }
    }
}
