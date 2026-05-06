using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class CategoriaRepositorio : BaseRepositorio, ICategoriaRepositorio
{
    public CategoriaRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Caterium> Lista(string? aplicacaoid) =>
        string.IsNullOrEmpty(aplicacaoid)
            ? _db.Cateria.AsNoTracking().OrderBy(c => c.Nome).ToList()
            : _db.Cateria.AsNoTracking().Where(c => c.Aplicacaoid == aplicacaoid).OrderBy(c => c.Nome).ToList();

    public Caterium? BuscaPorId(string id) =>
        _db.Cateria.AsNoTracking().FirstOrDefault(c => c.Cateriaid == id);

    public void Criar(Caterium item)
    {
        _db.Cateria.Add(item);
        _db.SaveChanges();
    }

    public void Atualizar(Caterium item)
    {
        _db.Cateria.Update(item);
        _db.SaveChanges();
    }

    public void Remover(Caterium item)
    {
        _db.Cateria.Remove(item);
        _db.SaveChanges();
    }
}
