using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class FaqRepositorio : BaseRepositorio, IFaqRepositorio
{
    public FaqRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Caterium>> ListaCategoriasPorAppAsync(string aplicacaoid) =>
        await _db.Cateria.AsNoTracking()
            .Where(c => c.Aplicacaoid == aplicacaoid)
            .ToListAsync();

    public async Task<IEnumerable<Formulario>> ListaFormulariosComCategoriaAsync(string aplicacaoid)
    {
        var areasIds = await _db.Areas.AsNoTracking()
            .Where(a => a.Aplicacaoid == aplicacaoid)
            .Select(a => a.Areaid)
            .ToListAsync();

        return await _db.Formularios.AsNoTracking()
            .Where(f => f.Categoriaid != null && f.Areaid != null && areasIds.Contains(f.Areaid))
            .ToListAsync();
    }

    public async Task<IEnumerable<Faq>> ListaFaqsAtivosAsync(IEnumerable<string> formularioIds)
    {
        var ids = formularioIds.ToList();
        return await _db.Faqs.AsNoTracking()
            .Where(faq => faq.Ativo && ids.Contains(faq.Formularioid))
            .OrderBy(faq => faq.Ordem)
            .ToListAsync();
    }

    public async Task<IEnumerable<Faq>> ListaPorFormularioAsync(string formularioid) =>
        await _db.Faqs.AsNoTracking()
            .Where(f => f.Formularioid == formularioid)
            .OrderBy(f => f.Ordem)
            .ToListAsync();

    public async Task<Faq?> BuscaPorIdAsync(string id) =>
        await _db.Faqs.FirstOrDefaultAsync(f => f.Faqid == id);

    public async Task<bool> TemAcessoAsync(string? formularioid, string? claimAppId)
    {
        if (string.IsNullOrEmpty(formularioid)) return false;
        return await _db.Formularios.AsNoTracking()
            .Where(f => f.Formularioid == formularioid)
            .Join(_db.Areas.AsNoTracking(), f => f.Areaid, a => a.Areaid, (f, a) => a)
            .AnyAsync(a => a.Aplicacaoid == claimAppId);
    }

    public async Task CriarAsync(Faq item)
    {
        _db.Faqs.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Faq item)
    {
        _db.Faqs.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Faq item)
    {
        _db.Faqs.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<Formularionew?> BuscaRespostaInboxAsync(int idform) =>
        await _db.Formularionews.FirstOrDefaultAsync(f => f.Idform == idform);
}
