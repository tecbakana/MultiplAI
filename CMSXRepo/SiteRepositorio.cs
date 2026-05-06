using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class SiteRepositorio : BaseRepositorio, ISiteRepositorio
{
    public SiteRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<Aplicacao?> BuscaPorSlugAsync(string slug) =>
        await _db.Aplicacaos.AsNoTracking().FirstOrDefaultAsync(a => a.Url == slug);

    public async Task<IEnumerable<Area>> ListaAreasAsync(string aplicacaoid) =>
        await _db.Areas
            .AsNoTracking()
            .Where(a => a.Aplicacaoid == aplicacaoid)
            .OrderBy(a => a.Posicao).ThenBy(a => a.Nome)
            .ToListAsync();

    public async Task<IEnumerable<Conteudo>> ListaConteudosPorAreaAsync(string areaid, int limite) =>
        await _db.Conteudos
            .AsNoTracking()
            .Where(c => c.Areaid == areaid)
            .OrderByDescending(c => c.Datainclusao)
            .Take(limite)
            .ToListAsync();

    public async Task<IEnumerable<ProdutoPublico>> ListaProdutosAsync(string aplicacaoid, string? cateriaid, int limite)
    {
        var produtos = await _db.Produtos
            .AsNoTracking()
            .Where(p => p.Aplicacaoid == aplicacaoid &&
                (cateriaid == null || p.Cateriaid == cateriaid))
            .Take(limite)
            .Select(p => new { p.Produtoid, p.Nome, p.Descricacurta, p.Valor })
            .ToListAsync();

        var ids = produtos.Select(p => p.Produtoid).ToList();
        var imagens = await _db.Imagems
            .AsNoTracking()
            .Where(i => ids.Contains(i.Parentid))
            .GroupBy(i => i.Parentid)
            .Select(g => new { Parentid = g.Key, Url = g.OrderBy(i => i.Imagemid).Select(i => i.Url).FirstOrDefault() })
            .ToListAsync();

        return produtos.Select(p => new ProdutoPublico(
            p.Produtoid,
            p.Nome,
            p.Descricacurta,
            p.Valor,
            imagens.FirstOrDefault(i => i.Parentid == p.Produtoid)?.Url
        )).ToList();
    }

    public async Task<IEnumerable<Caterium>> ListaCategoriasAsync(string aplicacaoid, string? cateriaidpai) =>
        await _db.Cateria
            .AsNoTracking()
            .Where(c => c.Aplicacaoid == aplicacaoid &&
                (string.IsNullOrEmpty(cateriaidpai) ? c.Cateriaidpai == null : c.Cateriaidpai == cateriaidpai))
            .ToListAsync();

    public async Task<IEnumerable<Faq>> ListaFaqsAtivosAsync(string formularioid) =>
        await _db.Faqs
            .AsNoTracking()
            .Where(f => f.Formularioid == formularioid && f.Ativo)
            .OrderBy(f => f.Ordem)
            .ToListAsync();

    public async Task<Formulario?> BuscaFormularioAsync(string formularioid) =>
        await _db.Formularios.AsNoTracking().FirstOrDefaultAsync(f => f.Formularioid == formularioid);

    public async Task<IEnumerable<Area>> ListaAreasMenuAsync(string aplicacaoid) =>
        await _db.Areas
            .AsNoTracking()
            .Where(a => a.Aplicacaoid == aplicacaoid && !string.IsNullOrEmpty(a.Url))
            .OrderBy(a => a.Posicao).ThenBy(a => a.Nome)
            .ToListAsync();
}
