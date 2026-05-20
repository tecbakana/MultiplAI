using CMSXData.Models;
using System.Threading.Tasks;

namespace ICMSX;

public interface ILojaRepositorio
{
    Task<Aplicacao?> ResolveAplicacaoAsync(string slug);
    Task<string?> ResolvePublicTokenAsync(string token);
    Task<string?> GetActiveTokenForAppAsync(string aplicacaoid);
    Task<IEnumerable<Produto>> ListaCatalogoAsync(string aplicacaoid);
    Task<Pedido> CriaPedidoAsync(CriarPedidoLojaInput input);
    Task AtualizaStatusPedidoAsync(Pedido pedido, string status, string descricao);
    Task<Pedido?> BuscaPedidoComTimelineAsync(Guid pedidoId);
    Task<IEnumerable<Pedido>> ListaPedidosPorClienteAsync(string clienteEmail);
    public record CriarPedidoLojaInput(
        string Aplicacaoid,
        string Numeropedido,
        string? Clientenome,
        string Clienteemail,
        decimal Valorpedido,
        string? MetodoPagamento
    );
}
