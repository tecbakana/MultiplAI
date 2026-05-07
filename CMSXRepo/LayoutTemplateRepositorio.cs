using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class LayoutTemplateRepositorio : BaseRepositorio, ILayoutTemplateRepositorio
{
    public LayoutTemplateRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<LayoutTemplate>> ListaAsync() =>
        await _db.LayoutTemplates.AsNoTracking().OrderBy(t => t.Nome).ToListAsync();

    public async Task<LayoutTemplate?> BuscaPorIdAsync(string id) =>
        await _db.LayoutTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Templateid == id);

    public async Task<LayoutTemplate?> BuscaPadraoAsync(string tipo) =>
        await _db.LayoutTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Tipo == tipo && t.Padrao);

    public async Task DesmarcarPadraoDoTipoAsync(string tipo, string? excluirId)
    {
        var anteriores = string.IsNullOrEmpty(excluirId)
            ? await _db.LayoutTemplates.Where(t => t.Tipo == tipo && t.Padrao).ToListAsync()
            : await _db.LayoutTemplates.Where(t => t.Tipo == tipo && t.Padrao && t.Templateid != excluirId).ToListAsync();

        foreach (var t in anteriores)
            t.Padrao = false;
    }

    public async Task CriarAsync(LayoutTemplate template)
    {
        _db.LayoutTemplates.Add(template);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(LayoutTemplate template)
    {
        _db.LayoutTemplates.Update(template);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(LayoutTemplate template)
    {
        _db.LayoutTemplates.Remove(template);
        await _db.SaveChangesAsync();
    }
}
