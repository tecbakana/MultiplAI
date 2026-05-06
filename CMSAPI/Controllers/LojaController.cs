using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMSAPI.Services;
using CMSXData.Models;
using ICMSX;
using System.Security.Claims;

namespace CMSAPI.Controllers;

[ApiController]
[Route("api/loja")]
[AllowAnonymous]
public class LojaController(
    ISalematicHttpService salematic,
    ILojaRepositorio lojaRepo,
    IClienteLojaRepositorio clienteLojaRepo,
    IEventPublisher publisher) : ControllerBase
{

    [HttpGet("resolve")]
    public IActionResult Resolve([FromQuery] string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return BadRequest(new { message = "slug é obrigatório." });

        var app = lojaRepo.ResolveAplicacao(slug);
        if (app == null || string.IsNullOrEmpty(app.Aplicacaoid))
            return NotFound(new { message = $"Site '{slug}' não encontrado." });

        var token = lojaRepo.GetActiveTokenForApp(app.Aplicacaoid);
        if (token == null)
            return NotFound(new { message = "Loja não disponível." });

        return Ok(new { token, nomeLoja = app.Nome });
    }

    [HttpGet("catalogo")]
    public async Task<IActionResult> GetProdutos([FromQuery] string aplicacaoid)
    {
        if (string.IsNullOrEmpty(aplicacaoid))
            return BadRequest(new { message = "aplicacaoid é obrigatório." });

        var produtos = await lojaRepo.ListaCatalogoAsync(aplicacaoid);

        return Ok(produtos.Select(p => new {
            p.Produtoid,
            p.Sku,
            p.Nome,
            p.Descricacurta,
            p.Valor
        }));
    }

    [HttpPost("auth/registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarLojaRequest req)
    {
        if (string.IsNullOrEmpty(req.Aplicacaoid))
            return BadRequest(new { message = "aplicacaoid é obrigatório." });

        var auth = await salematic.RegistrarAsync(req);
        if (auth is null)
            return StatusCode(502, new { message = "Falha ao registrar cliente na Salematic." });

        dynamic connProps = new System.Dynamic.ExpandoObject();
        connProps.banco = "SqlServer";
        connProps.parms = 3;
        clienteLojaRepo.MakeConnection(connProps);
        clienteLojaRepo.CriaClienteLoja(new ClienteLoja
        {
            Aplicacaoid        = req.Aplicacaoid,
            SalematicClienteId = auth.ClienteId
        });

        return Ok(auth);
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginLojaRequest req)
    {
        var auth = await salematic.LoginAsync(req);
        if (auth is null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        return Ok(auth);
    }

    [Authorize(AuthenticationSchemes = "Salematic")]
    [HttpPost("pedidos")]
    public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoLojaRequest req)
    {
        if (string.IsNullOrEmpty(req.Numeropedido) || string.IsNullOrEmpty(req.Clienteemail))
            return BadRequest(new { message = "numeropedido e clienteemail são obrigatórios." });

        var pedido = new Pedido
        {
            Aplicacaoid     = req.Aplicacaoid,
            Numeropedido    = req.Numeropedido,
            Clientenome     = req.Clientenome,
            Clienteemail    = req.Clienteemail,
            Valorpedido     = req.Valorpedido,
            MetodoPagamento = req.MetodoPagamento
        };

        await lojaRepo.CriaPedidoAsync(pedido);

        try
        {
            await publisher.PublicarPedidoAsync(pedido);
            await lojaRepo.AtualizaStatusPedidoAsync(pedido, "criado", "Pedido publicado no Service Bus com sucesso.");
        }
        catch (Exception)
        {
            await lojaRepo.AtualizaStatusPedidoAsync(pedido, "erro_envio", "Falha ao publicar no Service Bus. Pedido pendente de reenvio.");
        }

        return Created($"/api/loja/pedidos/{pedido.Pedidoid}/timeline", new
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
    public IActionResult MeusPedidos()
    {
        var clienteEmail = User.FindFirst("email")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(clienteEmail))
            return Unauthorized(new { message = "Email nao encontrado" });

        var pedidos = lojaRepo.ListaPedidosPorCliente(clienteEmail)
            .Select(p => new {
                p.Pedidoid,
                p.Aplicacaoid,
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
