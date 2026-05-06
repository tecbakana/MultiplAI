using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class SiteRepositorio : BaseRepositorio, ISiteRepositorio
{
    public SiteRepositorio(CmsxDbContext db) : base(db) { }

    public Aplicacao? BuscaPorSlug(string slug) =>
        _db.Aplicacaos.AsNoTracking().FirstOrDefault(a => a.Url == slug);

    public IEnumerable<Area> ListaAreas(string aplicacaoid) =>
        _db.Areas
            .AsNoTracking()
            .Where(a => a.Aplicacaoid == aplicacaoid)
            .OrderBy(a => a.Posicao).ThenBy(a => a.Nome)
            .ToList();

    public IEnumerable<Conteudo> ListaConteudosPorArea(string areaid, int limite) =>
        _db.Conteudos
            .AsNoTracking()
            .Where(c => c.Areaid == areaid)
            .OrderByDescending(c => c.Datainclusao)
            .Take(limite)
            .ToList();

    public IEnumerable<ProdutoPublico> ListaProdutos(string aplicacaoid, string? cateriaid, int limite)
    {
        var produtos = _db.Produtos
            .AsNoTracking()
            .Where(p => p.Aplicacaoid == aplicacaoid &&
                (cateriaid == null || p.Cateriaid == cateriaid))
            .Take(limite)
            .Select(p => new { p.Produtoid, p.Nome, p.Descricacurta, p.Valor })
            .ToList();

        var ids = produtos.Select(p => p.Produtoid).ToList();
        var imagens = _db.Imagems
            .AsNoTracking()
            .Where(i => ids.Contains(i.Parentid))
            .GroupBy(i => i.Parentid)
            .Select(g => new { Parentid = g.Key, Url = g.OrderBy(i => i.Imagemid).Select(i => i.Url).FirstOrDefault() })
            .ToList();

        return produtos.Select(p => new ProdutoPublico(
            p.Produtoid,
            p.Nome,
            p.Descricacurta,
            p.Valor,
            imagens.FirstOrDefault(i => i.Parentid == p.Produtoid)?.Url
        )).ToList();
    }

    public IEnumerable<Caterium> ListaCategorias(string aplicacaoid, string? cateriaidpai) =>
        _db.Cateria
            .AsNoTracking()
            .Where(c => c.Aplicacaoid == aplicacaoid &&
                (string.IsNullOrEmpty(cateriaidpai) ? c.Cateriaidpai == null : c.Cateriaidpai == cateriaidpai))
            .ToList();

    public IEnumerable<Faq> ListaFaqsAtivos(string formularioid) =>
        _db.Faqs
            .AsNoTracking()
            .Where(f => f.Formularioid == formularioid && f.Ativo)
            .OrderBy(f => f.Ordem)
            .ToList();

    public Formulario? BuscaFormulario(string formularioid) =>
        _db.Formularios.AsNoTracking().FirstOrDefault(f => f.Formularioid == formularioid);

    public IEnumerable<Area> ListaAreasMenu(string aplicacaoid) =>
        _db.Areas
            .AsNoTracking()
            .Where(a => a.Aplicacaoid == aplicacaoid && !string.IsNullOrEmpty(a.Url))
            .OrderBy(a => a.Posicao).ThenBy(a => a.Nome)
            .ToList();
}
