using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class GrupoRepositorio : BaseRepositorio, IGrupoRepositorio
{
    public GrupoRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Grupo> Lista() =>
        _db.Grupos.AsNoTracking().OrderBy(g => g.Nome).ToList();

    public Grupo? BuscaPorId(string id) =>
        _db.Grupos.AsNoTracking().FirstOrDefault(g => g.Grupoid == id);

    public IEnumerable<object> UsuariosPorGrupo(string grupoid) =>
        _db.Relusuariogrupos.AsNoTracking()
            .Where(r => r.Grupoid == grupoid)
            .Join(_db.Usuarios.AsNoTracking(), r => r.Usuarioid, u => u.Userid,
                (r, u) => (object)new { r.Relacaoid, u.Userid, u.Nome, u.Sobrenome, u.Apelido, u.Ativo })
            .ToList();

    public bool ExisteVinculoUsuario(string grupoid, string usuarioid) =>
        _db.Relusuariogrupos.Any(r => r.Grupoid == grupoid && r.Usuarioid == usuarioid);

    public void Criar(Grupo grupo)
    {
        _db.Grupos.Add(grupo);
        _db.SaveChanges();
    }

    public void Atualizar(Grupo grupo)
    {
        _db.Grupos.Update(grupo);
        _db.SaveChanges();
    }

    public void RemoverComVinculos(Grupo grupo)
    {
        var vinculos = _db.Relusuariogrupos.Where(r => r.Grupoid == grupo.Grupoid).ToList();
        _db.Relusuariogrupos.RemoveRange(vinculos);
        _db.Grupos.Remove(grupo);
        _db.SaveChanges();
    }

    public void AdicionarUsuario(Relusuariogrupo rel)
    {
        _db.Relusuariogrupos.Add(rel);
        _db.SaveChanges();
    }

    public Relusuariogrupo? BuscaVinculoPorRelacaoid(string relacaoid) =>
        _db.Relusuariogrupos.FirstOrDefault(r => r.Relacaoid == relacaoid);

    public void RemoverVinculoUsuario(Relusuariogrupo rel)
    {
        _db.Relusuariogrupos.Remove(rel);
        _db.SaveChanges();
    }
}
