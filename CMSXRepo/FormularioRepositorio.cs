using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class FormularioRepositorio : BaseRepositorio, IFormularioRepositorio
{
    public FormularioRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Formulario> ListaDefs(string? aplicacaoid, string? areaid)
    {
        var q = _db.Formularios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
        {
            var areasIds = _db.Areas.AsNoTracking()
                .Where(a => a.Aplicacaoid == aplicacaoid)
                .Select(a => a.Areaid)
                .ToHashSet();
            q = q.Where(f => f.Areaid != null && areasIds.Contains(f.Areaid));
        }

        if (!string.IsNullOrEmpty(areaid))
            q = q.Where(f => f.Areaid == areaid);

        return q.OrderBy(f => f.Nome)
                .Select(f => new Formulario
                {
                    Formularioid = f.Formularioid,
                    Nome         = f.Nome,
                    Valor        = f.Valor,
                    Ativo        = f.Ativo,
                    Datainclusao = f.Datainclusao,
                    Areaid       = f.Areaid,
                    Categoriaid  = f.Categoriaid
                })
                .ToList();
    }

    public Formulario? BuscaDefPorId(string id) =>
        _db.Formularios.AsNoTracking().FirstOrDefault(f => f.Formularioid == id);

    public string? AplicacaoidDaArea(string? areaid)
    {
        if (string.IsNullOrEmpty(areaid)) return null;
        return _db.Areas.AsNoTracking()
            .Where(a => a.Areaid == areaid)
            .Select(a => a.Aplicacaoid)
            .FirstOrDefault();
    }

    public void CriarDef(Formulario item)
    {
        _db.Formularios.Add(item);
        _db.SaveChanges();
    }

    public void AtualizarDef(Formulario item)
    {
        _db.Formularios.Update(item);
        _db.SaveChanges();
    }

    public void RemoverDef(Formulario item)
    {
        _db.Formularios.Remove(item);
        _db.SaveChanges();
    }

    public Formulario? BuscaFormularioPorId(string formularioid) =>
        _db.Formularios.AsNoTracking().FirstOrDefault(f => f.Formularioid == formularioid);

    public void Submeter(Formularionew item)
    {
        _db.Formularionews.Add(item);
        _db.SaveChanges();
    }

    public IEnumerable<Formularionew> ListaRespostas(string? aplicacaoid)
    {
        var q = _db.Formularionews.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
        {
            var formularioIds = _db.Formularios.AsNoTracking()
                .Where(f => _db.Areas.AsNoTracking()
                    .Where(a => a.Aplicacaoid == aplicacaoid)
                    .Select(a => a.Areaid)
                    .Contains(f.Areaid))
                .Select(f => f.Formularioid)
                .ToHashSet();
            q = q.Where(r => r.Formularioid != null && formularioIds.Contains(r.Formularioid));
        }

        return q.OrderByDescending(f => f.Idform).ToList();
    }

    public Formularionew? BuscaRespostaPorId(int id) =>
        _db.Formularionews.FirstOrDefault(f => f.Idform == id);

    public string? AplicacaoidDaResposta(string? formularioid)
    {
        if (string.IsNullOrEmpty(formularioid)) return null;
        return _db.Formularios.AsNoTracking()
            .Where(f => f.Formularioid == formularioid)
            .Join(_db.Areas.AsNoTracking(), f => f.Areaid, a => a.Areaid, (f, a) => a.Aplicacaoid)
            .FirstOrDefault();
    }

    public void AtualizarRespostaAtivo(Formularionew item)
    {
        _db.Formularionews.Update(item);
        _db.SaveChanges();
    }

    public void RemoverResposta(Formularionew item)
    {
        _db.Formularionews.Remove(item);
        _db.SaveChanges();
    }
}
