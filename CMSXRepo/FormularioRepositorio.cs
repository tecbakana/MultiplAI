using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class FormularioRepositorio : BaseRepositorio, IFormularioRepositorio
{
    public FormularioRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Formulario>> ListaDefsAsync(string? aplicacaoid, string? areaid)
    {
        var q = _db.Formularios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
        {
            var areasIds = await _db.Areas.AsNoTracking()
                .Where(a => a.Aplicacaoid == aplicacaoid)
                .Select(a => a.Areaid)
                .ToListAsync();
            q = q.Where(f => f.Areaid != null && areasIds.Contains(f.Areaid));
        }

        if (!string.IsNullOrEmpty(areaid))
            q = q.Where(f => f.Areaid == areaid);

        return await q.OrderBy(f => f.Nome)
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
                .ToListAsync();
    }

    public async Task<Formulario?> BuscaDefPorIdAsync(string id) =>
        await _db.Formularios.AsNoTracking().FirstOrDefaultAsync(f => f.Formularioid == id);

    public async Task<string?> AplicacaoidDaAreaAsync(string? areaid)
    {
        if (string.IsNullOrEmpty(areaid)) return null;
        return await _db.Areas.AsNoTracking()
            .Where(a => a.Areaid == areaid)
            .Select(a => a.Aplicacaoid)
            .FirstOrDefaultAsync();
    }

    public async Task CriarDefAsync(Formulario item)
    {
        _db.Formularios.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarDefAsync(Formulario item)
    {
        _db.Formularios.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverDefAsync(Formulario item)
    {
        _db.Formularios.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<Formulario?> BuscaFormularioPorIdAsync(string formularioid) =>
        await _db.Formularios.AsNoTracking().FirstOrDefaultAsync(f => f.Formularioid == formularioid);

    public async Task SubmeterAsync(Formularionew item)
    {
        _db.Formularionews.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Formularionew>> ListaRespostasAsync(string? aplicacaoid)
    {
        var q = _db.Formularionews.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
        {
            var formularioIds = (await _db.Formularios.AsNoTracking()
                .Where(f => _db.Areas.AsNoTracking()
                    .Where(a => a.Aplicacaoid == aplicacaoid)
                    .Select(a => a.Areaid)
                    .Contains(f.Areaid))
                .Select(f => f.Formularioid)
                .ToListAsync()).ToHashSet();
            q = q.Where(r => r.Formularioid != null && formularioIds.Contains(r.Formularioid));
        }

        return await q.OrderByDescending(f => f.Idform).ToListAsync();
    }

    public async Task<Formularionew?> BuscaRespostaPorIdAsync(int id) =>
        await _db.Formularionews.FirstOrDefaultAsync(f => f.Idform == id);

    public async Task<string?> AplicacaoidDaRespostaAsync(string? formularioid)
    {
        if (string.IsNullOrEmpty(formularioid)) return null;
        return await _db.Formularios.AsNoTracking()
            .Where(f => f.Formularioid == formularioid)
            .Join(_db.Areas.AsNoTracking(), f => f.Areaid, a => a.Areaid, (f, a) => a.Aplicacaoid)
            .FirstOrDefaultAsync();
    }

    public async Task AtualizarRespostaAtivoAsync(Formularionew item)
    {
        _db.Formularionews.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverRespostaAsync(Formularionew item)
    {
        _db.Formularionews.Remove(item);
        await _db.SaveChangesAsync();
    }
}
