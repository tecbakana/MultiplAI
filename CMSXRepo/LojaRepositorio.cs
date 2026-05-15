using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;
using static ICMSX.ILojaRepositorio;

namespace CMSXRepo;

public class LojaRepositorio : BaseRepositorio, ILojaRepositorio
{
    public LojaRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<Aplicacao?> ResolveAplicacaoAsync(string slug) =>
        await _db.Aplicacaos.FirstOrDefaultAsync(a => a.Url == slug);

    public async Task<string?> ResolvePublicTokenAsync(string token) =>
        await _db.PublicTokens
            .Where(t => t.Token == token && t.Ativo &&
                   (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow))
            .Select(t => t.Aplicacaoid)
            .FirstOrDefaultAsync();

    public async Task<string?> GetActiveTokenForAppAsync(string aplicacaoid) =>
        await _db.PublicTokens
            .Where(t => t.Aplicacaoid == aplicacaoid && t.Ativo &&
                   (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow))
            .OrderByDescending(t => t.Datainclusao)
            .Select(t => t.Token)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<Produto>> ListaCatalogoAsync(string aplicacaoid) =>
        await _db.Produtos
            .Where(p => p.Aplicacaoid == aplicacaoid)
            .ToListAsync();

    public async Task<Pedido> CriaPedidoAsync(CriarPedidoLojaInput input)
    {
        var pedido = new Pedido
        {
            Pedidoid = Guid.NewGuid(),
            Aplicacaoid = input.Aplicacaoid,
            Numeropedido = input.Numeropedido,
            Clientenome = input.Clientenome,
            Clienteemail = input.Clienteemail,
            Valorpedido = input.Valorpedido,
            MetodoPagamento = input.MetodoPagamento,
            Statusatual = "pendente",
            Datainclusao = DateTime.UtcNow
        };

        _db.Pedidos.Add(pedido);
        _db.Statuspedidos.Add(new Statuspedido
        {
            Statuspedidoid = Guid.NewGuid(),
            Pedidoid = pedido.Pedidoid,
            Status = "pendente",
            Descricao = "Pedido recebido.",
            Datahora = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return pedido;
    }

    public async Task AtualizaStatusPedidoAsync(Pedido pedido, string status, string descricao)
    {
        pedido.Statusatual = status;
        _db.Statuspedidos.Add(new Statuspedido
        {
            Statuspedidoid = Guid.NewGuid(),
            Pedidoid = pedido.Pedidoid,
            Status = status,
            Descricao = descricao,
            Datahora = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task<Pedido?> BuscaPedidoComTimelineAsync(Guid pedidoId) =>
        await _db.Pedidos
            .Include(p => p.Statuspedidos)
            .FirstOrDefaultAsync(p => p.Pedidoid == pedidoId);

    public async Task<IEnumerable<Pedido>> ListaPedidosPorClienteAsync(string clienteEmail) =>
        await _db.Pedidos
            .AsNoTracking()
            .Where(p => p.Clienteemail == clienteEmail)
            .OrderByDescending(p => p.Datainclusao)
            .ToListAsync();
}
