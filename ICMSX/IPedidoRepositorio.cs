using CMSXData.Models;

namespace ICMSX;

public interface IPedidoRepositorio
{
    Task<IEnumerable<Pedido>> ListaAsync(string? aplicacaoid, string? statusFiltro);
    Pedido? BuscaPorId(Guid id);
    Task<IEnumerable<Statuspedido>> ListaTimelineAsync(Guid pedidoid);
    Task CriarAsync(Pedido pedido, Statuspedido statusInicial);
    Task AtualizarStatusAsync(Pedido pedido, Statuspedido novoStatus);
}
