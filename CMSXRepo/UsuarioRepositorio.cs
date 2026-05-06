using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class UsuarioRepositorio : BaseRepositorio, IUsuarioRepositorio
{
    public UsuarioRepositorio(CmsxDbContext db) : base(db) { }

    private HashSet<string?> AdminIds() =>
        _db.Relusuariogrupos.AsNoTracking()
            .Join(_db.Grupos.AsNoTracking(), r => r.Grupoid, g => g.Grupoid, (r, g) => new { r.Usuarioid, g.Acessototal })
            .Where(x => x.Acessototal)
            .Select(x => x.Usuarioid)
            .ToHashSet();

    public IEnumerable<object> ListaTodos()
    {
        return _db.Usuarios.AsNoTracking()
            .Select(u => (object)new { u.Userid, u.Nome, u.Sobrenome, u.Apelido, u.Ativo, u.Datainclusao })
            .ToList();
    }

    public IEnumerable<object> ListaPorAplicacao(string aplicacaoid)
    {
        var adminIds = AdminIds();
        return _db.Relusuarioaplicacaos.AsNoTracking()
            .Where(r => r.Aplicacaoid == aplicacaoid)
            .Join(_db.Usuarios.AsNoTracking(), r => r.Usuarioid, u => u.Userid,
                (r, u) => new { u.Userid, u.Nome, u.Sobrenome, u.Apelido, u.Ativo, u.Datainclusao })
            .AsEnumerable()
            .Where(u => !adminIds.Contains(u.Userid))
            .Cast<object>()
            .ToList();
    }

    public Usuario? BuscaPorId(string id) =>
        _db.Usuarios.AsNoTracking().FirstOrDefault(u => u.Userid == id);

    public bool PertenceAplicacao(string userid, string aplicacaoid) =>
        _db.Relusuarioaplicacaos.Any(r => r.Usuarioid == userid && r.Aplicacaoid == aplicacaoid);

    public void Criar(Usuario usuario, Relusuarioaplicacao? vinculo)
    {
        _db.Usuarios.Add(usuario);
        if (vinculo != null)
            _db.Relusuarioaplicacaos.Add(vinculo);
        _db.SaveChanges();
    }

    public void Atualizar(Usuario usuario)
    {
        _db.Usuarios.Update(usuario);
        _db.SaveChanges();
    }

    public void Remover(Usuario usuario)
    {
        _db.Usuarios.Remove(usuario);
        _db.SaveChanges();
    }
}
