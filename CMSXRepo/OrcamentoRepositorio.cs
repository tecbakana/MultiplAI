using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class OrcamentoRepositorio : BaseRepositorio, IOrcamentoRepositorio
{
    public OrcamentoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<OrcamentoCabecalho>> ListaAsync(string aplicacaoid) =>
        await _db.OrcamentoCabecalhos
            .AsNoTracking()
            .Where(o => o.Aplicacaoid == aplicacaoid)
            .OrderByDescending(o => o.Datainclusao)
            .ToListAsync();

    public async Task<OrcamentoCabecalho?> BuscaPorIdAsync(Guid id) =>
        await _db.OrcamentoCabecalhos
            .Include(o => o.OrcamentoDetalhes)
            .Include(o => o.OrcamentoDetalheCompostos)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Orcamentoid == id);

    public async Task CriarAsync(OrcamentoCabecalho cabecalho, IEnumerable<OrcamentoDetalhe> itens)
    {
        _db.OrcamentoCabecalhos.Add(cabecalho);
        _db.OrcamentoDetalhes.AddRange(itens);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Produto>> ListaProdutosPublicosAsync(string aplicacaoid) =>
        await _db.Produtos
            .AsNoTracking()
            .Where(p => p.Aplicacaoid == aplicacaoid)
            .ToListAsync();

    public async Task ToggleAprovadoAsync(OrcamentoCabecalho orcamento)
    {
        orcamento.Aprovado = !orcamento.Aprovado;
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(OrcamentoCabecalho orcamento)
    {
        _db.OrcamentoDetalhes.RemoveRange(orcamento.OrcamentoDetalhes);
        _db.OrcamentoCabecalhos.Remove(orcamento);
        await _db.SaveChangesAsync();
    }
}
