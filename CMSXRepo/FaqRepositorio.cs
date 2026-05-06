using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class FaqRepositorio : BaseRepositorio, IFaqRepositorio
{
    public FaqRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Caterium> ListaCategoriasPorApp(string aplicacaoid) =>
        _db.Cateria.AsNoTracking()
            .Where(c => c.Aplicacaoid == aplicacaoid)
            .ToList();

    public IEnumerable<Formulario> ListaFormulariosComCategoria(string aplicacaoid)
    {
        var areasIds = _db.Areas.AsNoTracking()
            .Where(a => a.Aplicacaoid == aplicacaoid)
            .Select(a => a.Areaid)
            .ToHashSet();

        return _db.Formularios.AsNoTracking()
            .Where(f => f.Categoriaid != null && f.Areaid != null && areasIds.Contains(f.Areaid))
            .ToList();
    }

    public IEnumerable<Faq> ListaFaqsAtivos(IEnumerable<string> formularioIds)
    {
        var ids = formularioIds.ToList();
        return _db.Faqs.AsNoTracking()
            .Where(faq => faq.Ativo && ids.Contains(faq.Formularioid))
            .OrderBy(faq => faq.Ordem)
            .ToList();
    }

    public IEnumerable<Faq> ListaPorFormulario(string formularioid) =>
        _db.Faqs.AsNoTracking()
            .Where(f => f.Formularioid == formularioid)
            .OrderBy(f => f.Ordem)
            .ToList();

    public Faq? BuscaPorId(string id) =>
        _db.Faqs.FirstOrDefault(f => f.Faqid == id);

    public bool TemAcesso(string? formularioid, string? claimAppId)
    {
        if (string.IsNullOrEmpty(formularioid)) return false;
        return _db.Formularios.AsNoTracking()
            .Where(f => f.Formularioid == formularioid)
            .Join(_db.Areas.AsNoTracking(), f => f.Areaid, a => a.Areaid, (f, a) => a)
            .Any(a => a.Aplicacaoid == claimAppId);
    }

    public void Criar(Faq item)
    {
        _db.Faqs.Add(item);
        _db.SaveChanges();
    }

    public void Atualizar(Faq item)
    {
        _db.Faqs.Update(item);
        _db.SaveChanges();
    }

    public void Remover(Faq item)
    {
        _db.Faqs.Remove(item);
        _db.SaveChanges();
    }

    public Formularionew? BuscaRespostaInbox(int idform) =>
        _db.Formularionews.FirstOrDefault(f => f.Idform == idform);
}
