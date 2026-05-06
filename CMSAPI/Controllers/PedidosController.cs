using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class PedidosController : Controller
{
    private readonly IPedidoRepositorio _repo;
    private readonly IEventPublisher _publisher;

    public PedidosController(IPedidoRepositorio repo, IEventPublisher publisher)
    {
        _repo      = repo;
        _publisher = publisher;
    }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? aplicacaoid = null, [FromQuery] string? status = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = acessoTotal && !string.IsNullOrEmpty(aplicacaoid) ? aplicacaoid : claimAppId;

        var lista = await _repo.ListaAsync(appId, status);

        return Ok(lista.Select(p => new
        {
            p.Pedidoid,
            p.Aplicacaoid,
            p.Numeropedido,
            p.Clientenome,
            p.Clienteemail,
            p.Valorpedido,
            p.Statusatual,
            p.Datainclusao
        }));
    }

    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var pedido = _repo.BuscaPorId(id);
        if (pedido is null) return NotFound();
        if (!acessoTotal && pedido.Aplicacaoid != claimAppId) return Forbid();

        var timeline = await _repo.ListaTimelineAsync(id);

        return Ok(timeline.Select(s => new
        {
            s.Statuspedidoid,
            s.Status,
            s.Descricao,
            s.Datahora
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] NovoPedido dto)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = dto.Aplicacaoid ?? claimAppId;
        if (!acessoTotal && appId != claimAppId) return Forbid();

        var pedido = new Pedido
        {
            Pedidoid        = Guid.NewGuid(),
            Aplicacaoid     = appId,
            Numeropedido    = dto.Numeropedido,
            Clientenome     = dto.Clientenome,
            Clienteemail    = dto.Clienteemail,
            Valorpedido     = dto.Valorpedido,
            MetodoPagamento = dto.MetodoPagamento,
            Statusatual     = "aguardando_envio",
            Datainclusao    = DateTime.UtcNow
        };

        await _repo.CriarAsync(pedido, new Statuspedido
        {
            Statuspedidoid = Guid.NewGuid(),
            Pedidoid       = pedido.Pedidoid,
            Status         = "aguardando_envio",
            Descricao      = "Pedido recebido, aguardando envio ao processador.",
            Datahora       = DateTime.UtcNow
        });

        try
        {
            await _publisher.PublicarPedidoAsync(pedido);

            pedido.Statusatual = "criado";
            await _repo.AtualizarStatusAsync(pedido, new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = "criado",
                Descricao      = "Pedido publicado no Service Bus com sucesso.",
                Datahora       = DateTime.UtcNow
            });
        }
        catch (Exception)
        {
            pedido.Statusatual = "erro_envio";
            await _repo.AtualizarStatusAsync(pedido, new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = "erro_envio",
                Descricao      = "Falha ao publicar no Service Bus. Pedido pendente de reenvio.",
                Datahora       = DateTime.UtcNow
            });
        }

        return CreatedAtAction(nameof(GetTimeline), new { id = pedido.Pedidoid }, new
        {
            pedido.Pedidoid,
            pedido.Aplicacaoid,
            pedido.Numeropedido,
            pedido.Clientenome,
            pedido.Clienteemail,
            pedido.Valorpedido,
            pedido.Statusatual,
            pedido.Datainclusao
        });
    }

    [HttpPost("{id}/reenviar")]
    [AllowAnonymous]
    public async Task<IActionResult> Reenviar(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var pedido = _repo.BuscaPorId(id);
        if (pedido is null) return NotFound();
        if (!acessoTotal && pedido.Aplicacaoid != claimAppId) return Forbid();
        if (pedido.Statusatual != "erro_envio" && pedido.Statusatual != "aguardando_envio")
            return BadRequest(new { message = "Pedido não está pendente de reenvio." });

        try
        {
            await _publisher.PublicarPedidoAsync(pedido);

            pedido.Statusatual = "criado";
            await _repo.AtualizarStatusAsync(pedido, new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = "criado",
                Descricao      = "Pedido reenviado ao Service Bus com sucesso.",
                Datahora       = DateTime.UtcNow
            });
            return Ok(new { message = "Pedido reenviado com sucesso." });
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Falha ao publicar no Service Bus. Tente novamente." });
        }
    }

    public class NovoPedido
    {
        public string? Aplicacaoid { get; set; }
        public string? Numeropedido { get; set; }
        public string? Clientenome { get; set; }
        public string? Clienteemail { get; set; }
        public decimal? Valorpedido { get; set; }
        public string? MetodoPagamento { get; set; }
    }
}
