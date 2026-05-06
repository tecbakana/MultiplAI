using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class LayoutTemplateRepositorio : BaseRepositorio, ILayoutTemplateRepositorio
{
    public LayoutTemplateRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<LayoutTemplate> Lista() =>
        _db.LayoutTemplates.AsNoTracking().OrderBy(t => t.Nome).ToList();

    public LayoutTemplate? BuscaPorId(string id) =>
        _db.LayoutTemplates.AsNoTracking().FirstOrDefault(t => t.Templateid == id);

    public LayoutTemplate? BuscaPadrao(string tipo) =>
        _db.LayoutTemplates.AsNoTracking().FirstOrDefault(t => t.Tipo == tipo && t.Padrao);

    public void DesmarcarPadraoDoTipo(string tipo, string? excluirId)
    {
        var anteriores = string.IsNullOrEmpty(excluirId)
            ? _db.LayoutTemplates.Where(t => t.Tipo == tipo && t.Padrao).ToList()
            : _db.LayoutTemplates.Where(t => t.Tipo == tipo && t.Padrao && t.Templateid != excluirId).ToList();

        foreach (var t in anteriores)
            t.Padrao = false;
    }

    public void Criar(LayoutTemplate template)
    {
        _db.LayoutTemplates.Add(template);
        _db.SaveChanges();
    }

    public void Atualizar(LayoutTemplate template)
    {
        _db.LayoutTemplates.Update(template);
        _db.SaveChanges();
    }

    public void Remover(LayoutTemplate template)
    {
        _db.LayoutTemplates.Remove(template);
        _db.SaveChanges();
    }
}
