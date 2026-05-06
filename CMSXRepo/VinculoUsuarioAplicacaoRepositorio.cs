using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class VinculoUsuarioAplicacaoRepositorio : BaseRepositorio, IVinculoUsuarioAplicacaoRepositorio
{
    public VinculoUsuarioAplicacaoRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<object> Lista(string? aplicacaoid, string? usuarioid)
    {
        var q = _db.Relusuarioaplicacaos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(aplicacaoid))
            q = q.Where(r => r.Aplicacaoid == aplicacaoid);

        if (!string.IsNullOrEmpty(usuarioid))
            q = q.Where(r => r.Usuarioid == usuarioid);

        return q
            .Join(_db.Usuarios.AsNoTracking(), r => r.Usuarioid, u => u.Userid,
                (r, u) => new { r.Relacaoid, r.Aplicacaoid, r.Usuarioid, u.Nome, u.Sobrenome, u.Apelido, u.Ativo })
            .AsEnumerable()
            .Join(_db.Aplicacaos.AsNoTracking().AsEnumerable(), r => r.Aplicacaoid, a => a.Aplicacaoid,
                (r, a) => (object)new { r.Relacaoid, r.Usuarioid, r.Nome, r.Sobrenome, r.Apelido, r.Ativo, r.Aplicacaoid, AppNome = a.Nome })
            .ToList();
    }

    public bool ExisteVinculo(string usuarioid, string aplicacaoid) =>
        _db.Relusuarioaplicacaos.Any(r => r.Usuarioid == usuarioid && r.Aplicacaoid == aplicacaoid);

    public void Criar(Relusuarioaplicacao rel)
    {
        _db.Relusuarioaplicacaos.Add(rel);
        _db.SaveChanges();
    }

    public Relusuarioaplicacao? BuscaPorRelacaoid(string relacaoid) =>
        _db.Relusuarioaplicacaos.FirstOrDefault(r => r.Relacaoid == relacaoid);

    public void Remover(Relusuarioaplicacao rel)
    {
        _db.Relusuarioaplicacaos.Remove(rel);
        _db.SaveChanges();
    }
}
