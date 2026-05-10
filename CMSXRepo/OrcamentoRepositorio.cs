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
            .AsNoTracking()
            .Include(o => o.OrcamentoDetalhes)
            .Include(o => o.OrcamentoDetalheCompostos)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Orcamentoid == id);

    public async Task<Guid> CriarAsync(OrcamentoInput input)
    {
        var cabecalho = new OrcamentoCabecalho
        {
            Orcamentoid       = Guid.NewGuid(),
            Aplicacaoid       = input.Aplicacaoid,
            Nome              = input.Nome,
            Email             = input.Email,
            Telefone          = input.Telefone,
            Descricaoservico  = input.Descricaoservico,
            Valorestimado     = input.Valorestimado,
            Prazo             = input.Prazo,
            Nomevendedor      = input.Nomevendedor,
            Datainclusao      = DateTime.UtcNow
        };

        var itens = input.Itens.Select(i => new OrcamentoDetalhe
        {
            Orcamentodetalheid = Guid.NewGuid(),
            Orcamentoid        = cabecalho.Orcamentoid,
            Descricao          = i.Descricao,
            Quantidade         = i.Quantidade,
            Valor              = i.Valor,
            Ativo              = true
        });

        _db.OrcamentoCabecalhos.Add(cabecalho);
        _db.OrcamentoDetalhes.AddRange(itens);
        await _db.SaveChangesAsync();
        return cabecalho.Orcamentoid;
    }

    public async Task<bool> ToggleAprovadoAsync(Guid id)
    {
        var orcamento = await _db.OrcamentoCabecalhos
            .FirstOrDefaultAsync(o => o.Orcamentoid == id);
        if (orcamento == null) return false;

        orcamento.Aprovado = !orcamento.Aprovado;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        var orcamento = await _db.OrcamentoCabecalhos
            .Include(o => o.OrcamentoDetalhes)
            .Include(o => o.OrcamentoDetalheCompostos)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Orcamentoid == id);
        if (orcamento == null) return false;

        _db.OrcamentoDetalheCompostos.RemoveRange(orcamento.OrcamentoDetalheCompostos);
        _db.OrcamentoDetalhes.RemoveRange(orcamento.OrcamentoDetalhes);
        _db.OrcamentoCabecalhos.Remove(orcamento);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Produto>> ListaProdutosPublicosAsync(string aplicacaoid) =>
        await _db.Produtos
            .AsNoTracking()
            .Where(p => p.Aplicacaoid == aplicacaoid)
            .ToListAsync();
}
