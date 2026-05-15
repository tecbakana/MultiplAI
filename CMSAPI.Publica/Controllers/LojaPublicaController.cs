using CMSAPIPublica.Services;
using ICMSX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using static ICMSX.IClienteLojaRepositorio;
using static ICMSX.ILojaRepositorio;

namespace CMSAPIPublica.Controllers;

[ApiController]
[Route("api/publico/loja")]
[EnableRateLimiting("api_publica")]
public class LojaPublicaController(
    SalematicHttpService salematic,
    ILojaRepositorio lojaRepo,
    IClienteLojaRepositorio clienteLojaRepo,
    IEventPublisher publisher) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve([FromQuery] string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return BadRequest(new { message = "slug é obrigatório." });

        var app = await lojaRepo.ResolveAplicacaoAsync(slug);
        if (app == null)
            return NotFound(new { message = $"Site '{slug}' não encontrado." });

        var token = await lojaRepo.GetActiveTokenForAppAsync(app.Aplicacaoid!);
        if (token == null)
            return NotFound(new { message = "Loja não disponível." });

        return Ok(new { token, nomeLoja = app.Nome });
    }

    [AllowAnonymous]
    [HttpGet("catalogo")]
    public async Task<IActionResult> GetCatalogo([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "token é obrigatório." });

        var aplicacaoid = await lojaRepo.ResolvePublicTokenAsync(token);
        if (aplicacaoid == null)
            return NotFound(new { message = "Token inválido ou expirado." });

        var produtos = await lojaRepo.ListaCatalogoAsync(aplicacaoid);

        return Ok(produtos.Select(p => new {
            p.Produtoid,
            p.Sku,
            p.Nome,
            p.Descricacurta,
            p.Valor
        }));
    }

    [AllowAnonymous]
    [HttpPost("auth/registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarLojaRequest req)
    {
        if (string.IsNullOrEmpty(req.Token))
            return BadRequest(new { message = "token é obrigatório." });

        var aplicacaoid = await lojaRepo.ResolvePublicTokenAsync(req.Token);
        if (aplicacaoid == null)
            return BadRequest(new { message = "Token inválido ou expirado." });

        req.Aplicacaoid = aplicacaoid;

        var auth = await salematic.RegistrarAsync(req);
        if (auth is null)
            return StatusCode(502, new { message = "Falha ao registrar cliente na Salematic." });

        await clienteLojaRepo.CriaClienteLojaAsync(new CriaClienteLojaInput(aplicacaoid, auth.ClienteId));

        return Ok(auth);
    }

    [AllowAnonymous]
    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginLojaRequest req)
    {
        var auth = await salematic.LoginAsync(req);
        if (auth is null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        return Ok(auth);
    }

    [AllowAnonymous]
    [HttpPost("auth/esqueci-senha")]
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequest req)
    {
        if (string.IsNullOrEmpty(req.Email))
            return BadRequest(new { message = "E-mail é obrigatório." });

        await salematic.EsqueciSenhaAsync(req);
        return Ok(new { message = "Se o e-mail estiver cadastrado, você receberá as instruções em breve." });
    }

    [AllowAnonymous]
    [HttpPost("auth/reset-senha")]
    public async Task<IActionResult> ResetSenha([FromBody] ResetSenhaRequest req)
    {
        if (string.IsNullOrEmpty(req.Token) || string.IsNullOrEmpty(req.NovaSenha))
            return BadRequest(new { message = "Token e nova senha são obrigatórios." });

        var ok = await salematic.ResetSenhaAsync(req);
        if (!ok)
            return BadRequest(new { message = "Token inválido ou expirado." });

        return Ok(new { message = "Senha redefinida com sucesso." });
    }

    [Authorize(AuthenticationSchemes = "Salematic")]
    [HttpPost("pedidos")]
    public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoLojaRequest req)
    {
        if (string.IsNullOrEmpty(req.Token))
            return BadRequest(new { message = "token é obrigatório." });

        var aplicacaoid = await lojaRepo.ResolvePublicTokenAsync(req.Token);
        if (aplicacaoid == null)
            return BadRequest(new { message = "Token inválido ou expirado." });

        if (string.IsNullOrEmpty(req.Numeropedido) || string.IsNullOrEmpty(req.Clienteemail))
            return BadRequest(new { message = "numeropedido e clienteemail são obrigatórios." });

        var pedido = await lojaRepo.CriaPedidoAsync(new CriarPedidoLojaInput(
            aplicacaoid,
            req.Numeropedido,
            req.Clientenome,
            req.Clienteemail,
            req.Valorpedido,
            req.MetodoPagamento
        ));

        try
        {
            await publisher.PublicarPedidoAsync(pedido);
            await lojaRepo.AtualizaStatusPedidoAsync(pedido, "criado", "Pedido publicado no Service Bus com sucesso.");
        }
        catch (Exception)
        {
            await lojaRepo.AtualizaStatusPedidoAsync(pedido, "erro_envio", "Falha ao publicar no Service Bus. Pedido pendente de reenvio.");
        }

        return Created($"/api/publico/loja/pedidos/{pedido.Pedidoid}/timeline", new
        {
            pedido.Pedidoid,
            pedido.Numeropedido,
            pedido.Statusatual
        });
    }

    [Authorize(AuthenticationSchemes = "Salematic")]
    [HttpGet("pedidos/{id:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id)
    {
        var clienteEmail = User.FindFirst("email")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value;

        var pedido = await lojaRepo.BuscaPedidoComTimelineAsync(id);
        if (pedido is null)
            return NotFound(new { message = "Pedido não encontrado." });

        if (pedido.Clienteemail != clienteEmail)
            return Forbid();

        var timeline = pedido.Statuspedidos
            .OrderBy(s => s.Datahora)
            .Select(s => new { s.Status, s.Descricao, s.Datahora });

        return Ok(new
        {
            pedido.Pedidoid,
            pedido.Numeropedido,
            pedido.Statusatual,
            timeline
        });
    }

    [Authorize(AuthenticationSchemes = "Salematic")]
    [HttpGet("meus-pedidos")]
    public async Task<IActionResult> MeusPedidos()
    {
        var clienteEmail = User.FindFirst("email")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(clienteEmail))
            return Unauthorized(new { message = "Email não encontrado." });

        var pedidos = (await lojaRepo.ListaPedidosPorClienteAsync(clienteEmail))
            .Select(p => new {
                p.Pedidoid,
                p.Numeropedido,
                p.Clientenome,
                p.Clienteemail,
                p.Valorpedido,
                p.Statusatual,
                p.Datainclusao
            });

        return Ok(pedidos);
    }
}
