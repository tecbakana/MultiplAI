using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class AreasRepositorio : BaseRepositorio, IAreasRepositorio
{
    public AreasRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Area> Lista(string? aplicacaoid) =>
        string.IsNullOrEmpty(aplicacaoid)
            ? _db.Areas.AsNoTracking().OrderBy(a => a.Posicao).ToList()
            : _db.Areas.AsNoTracking().Where(a => a.Aplicacaoid == aplicacaoid).OrderBy(a => a.Posicao).ToList();

    public Area? BuscaPorId(string id) =>
        _db.Areas.AsNoTracking().FirstOrDefault(a => a.Areaid == id);

    public void Criar(Area area)
    {
        _db.Areas.Add(area);
        _db.SaveChanges();
    }

    public void Atualizar(Area area)
    {
        _db.Areas.Update(area);
        _db.SaveChanges();
    }

    public void Remover(Area area)
    {
        _db.Areas.Remove(area);
        _db.SaveChanges();
    }
}
