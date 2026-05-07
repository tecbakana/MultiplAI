using CMSXData.Models;

namespace ICMSX;

public interface ILojaRepositorio
{
    Task<Aplicacao?> ResolveAplicacaoAsync(string slug);
    Task<string?> ResolvePublicTokenAsync(string token);
    Task<string?> GetActiveTokenForAppAsync(string aplicacaoid);
    Task<IEnumerable<Produto>> ListaCatalogoAsync(string aplicacaoid);
    Task<Pedido> CriaPedidoAsync(Pedido pedido);
    Task AtualizaStatusPedidoAsync(Pedido pedido, string status, string descricao);
    Task<Pedido?> BuscaPedidoComTimelineAsync(Guid pedidoId);
    Task<IEnumerable<Pedido>> ListaPedidosPorClienteAsync(string clienteEmail);
}
