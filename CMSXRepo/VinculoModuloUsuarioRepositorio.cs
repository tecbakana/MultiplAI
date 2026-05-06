using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class VinculoModuloUsuarioRepositorio : BaseRepositorio, IVinculoModuloUsuarioRepositorio
{
    public VinculoModuloUsuarioRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<object> Lista(string? aplicacaoid, string? usuarioid)
    {
        IEnumerable<string?> usuarioIds = !string.IsNullOrEmpty(aplicacaoid)
            ? _db.Relusuarioaplicacaos.AsNoTracking()
                .Where(r => r.Aplicacaoid == aplicacaoid)
                .Select(r => r.Usuarioid)
                .ToList()
            : _db.Usuarios.AsNoTracking().Select(u => u.Userid).ToList();

        if (!string.IsNullOrEmpty(usuarioid))
            usuarioIds = usuarioIds.Where(id => id == usuarioid);

        var usuarioIdList = usuarioIds.ToList();

        return _db.Relmodulousuarios.AsNoTracking()
            .Where(r => usuarioIdList.Contains(r.Usuarioid))
            .Join(_db.Usuarios.AsNoTracking(), r => r.Usuarioid, u => u.Userid,
                (r, u) => new { r.Relacaoid, r.Moduloid, r.Usuarioid, nomeUsuario = u.Nome + " " + u.Sobrenome, apelido = u.Apelido })
            .Join(_db.Modulos.AsNoTracking(), x => x.Moduloid, m => m.Moduloid,
                (x, m) => (object)new
                {
                    x.Relacaoid, x.Moduloid, x.Usuarioid,
                    x.nomeUsuario, x.apelido,
                    nomeModulo = m.Nome, urlModulo = m.Url
                })
            .ToList();
    }

    public bool ExisteVinculo(string usuarioid, string moduloid) =>
        _db.Relmodulousuarios.Any(r => r.Usuarioid == usuarioid && r.Moduloid == moduloid);

    public void Criar(Relmodulousuario rel)
    {
        _db.Relmodulousuarios.Add(rel);
        _db.SaveChanges();
    }

    public Relmodulousuario? BuscaPorRelacaoid(string relacaoid) =>
        _db.Relmodulousuarios.FirstOrDefault(r => r.Relacaoid == relacaoid);

    public void Remover(Relmodulousuario rel)
    {
        _db.Relmodulousuarios.Remove(rel);
        _db.SaveChanges();
    }
}
