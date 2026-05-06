using Azure.Messaging.ServiceBus;
using CMSXData.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ICMSX;

namespace CMSAPI.Services
{
    public class PedidoServiceBusPublisher : IEventPublisher
    {

        private readonly IConfiguration _config;
        private readonly ILogger<PedidoServiceBusPublisher> _logger;
        private readonly ServiceBusClient? _client;
        private readonly ServiceBusSender? _sender;  

        public PedidoServiceBusPublisher(
            IConfiguration config,
            ILogger<PedidoServiceBusPublisher> logger)
        {
            _config = config;
            _logger = logger;
            var connStr  = _config["ServiceBus:ConnectionString"];
            var topico = _config["ServiceBus:TopicoPedidos"] ?? "top-pedidos";
            if (string.IsNullOrWhiteSpace(connStr))
            {
                _logger.LogWarning("ServiceBus:ConnectionString n�o configurada � publisher desabilitado.");
                return;
            }
            _client = new ServiceBusClient(connStr);
            _sender = _client.CreateSender(topico);
        }

        public async Task PublicarPedidoAsync(Pedido pedido)
        {
            if (_sender == null)
            {
                _logger.LogWarning("Publisher de pedidos n�o configurado � evento n�o publicado.");
                return;
            }
            var mensagem = new
            {
                aplicacaoid = pedido.Aplicacaoid,
                numeropedido = pedido.Numeropedido,
                clientenome = pedido.Clientenome,
                clienteemail = pedido.Clienteemail,
                valorpedido = pedido.Valorpedido,
                status = pedido.Statusatual?.ToLower() ?? string.Empty,
                descricao = $"Pedido {pedido.Statusatual?.ToLower() ?? string.Empty} no CMSX"
            };
            var json = JsonSerializer.Serialize(mensagem);
            var message = new ServiceBusMessage(json);
            try
            {
                await _sender.SendMessageAsync(message);
                _logger.LogInformation("Evento de pedido publicado: PedidoId={PedidoId} Status={Status}", pedido.Numeropedido, pedido.Statusatual);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento de pedido: PedidoId={PedidoId}", pedido.Numeropedido);
            }
        }
    }
}