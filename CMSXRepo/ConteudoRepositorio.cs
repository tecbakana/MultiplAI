using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ConteudoRepositorio : BaseRepositorio, IConteudoRepositorio
{
    public ConteudoRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Conteudo> Lista(string? aplicacaoid, string? areaid, string? cateriaid)
    {
        var q = _db.Conteudos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
        {
            var areasIds = _db.Areas
                .AsNoTracking()
                .Where(a => a.Aplicacaoid == aplicacaoid)
                .Select(a => a.Areaid)
                .ToHashSet();
            q = q.Where(c => c.Areaid != null && areasIds.Contains(c.Areaid));
        }

        if (!string.IsNullOrEmpty(areaid))
            q = q.Where(c => c.Areaid == areaid);

        if (!string.IsNullOrEmpty(cateriaid))
            q = q.Where(c => c.Cateriaid == cateriaid);

        return q.OrderByDescending(c => c.Datainclusao).ToList();
    }

    public Conteudo? BuscaPorId(string id) =>
        _db.Conteudos.AsNoTracking().FirstOrDefault(c => c.Conteudoid == id);

    public string? AplicacaoidDaArea(string? areaid)
    {
        if (string.IsNullOrEmpty(areaid)) return null;
        return _db.Areas.AsNoTracking()
            .Where(a => a.Areaid == areaid)
            .Select(a => a.Aplicacaoid)
            .FirstOrDefault();
    }

    public void Criar(Conteudo item)
    {
        _db.Conteudos.Add(item);
        _db.SaveChanges();
    }

    public void Atualizar(Conteudo item)
    {
        _db.Conteudos.Update(item);
        _db.SaveChanges();
    }

    public void Remover(Conteudo item)
    {
        _db.Conteudos.Remove(item);
        _db.SaveChanges();
    }
}
