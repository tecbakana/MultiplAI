using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class PedidoRepositorio : BaseRepositorio, IPedidoRepositorio
{
    public PedidoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Pedido>> ListaAsync(string? aplicacaoid, string? statusFiltro)
    {
        var query = _db.Pedidos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
            query = query.Where(p => p.Aplicacaoid == aplicacaoid);

        if (!string.IsNullOrEmpty(statusFiltro))
            query = query.Where(p => p.Statusatual == statusFiltro);

        return await query.OrderByDescending(p => p.Datainclusao).ToListAsync();
    }

    public async Task<Pedido?> BuscaPorIdAsync(Guid id) =>
        await _db.Pedidos.FirstOrDefaultAsync(p => p.Pedidoid == id);

    public async Task<IEnumerable<Statuspedido>> ListaTimelineAsync(Guid pedidoid) =>
        await _db.Statuspedidos
            .AsNoTracking()
            .Where(s => s.Pedidoid == pedidoid)
            .OrderBy(s => s.Datahora)
            .ToListAsync();

    public async Task CriarAsync(Pedido pedido, Statuspedido statusInicial)
    {
        _db.Pedidos.Add(pedido);
        _db.Statuspedidos.Add(statusInicial);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarStatusAsync(Pedido pedido, Statuspedido novoStatus)
    {
        _db.Statuspedidos.Add(novoStatus);
        await _db.SaveChangesAsync();
    }
}
