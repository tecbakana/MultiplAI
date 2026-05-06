using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ModuloRepositorio : BaseRepositorio, IModuloRepositorio
{
    public ModuloRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Modulo> ListaTodos() =>
        _db.Modulos.AsNoTracking().OrderBy(m => m.Posicao).ToList();

    public IEnumerable<Modulo> ListaPorAplicacao(string aplicacaoid) =>
        _db.Relmoduloaplicacaos.AsNoTracking()
            .Where(r => r.Aplicacaoid == aplicacaoid)
            .Join(_db.Modulos.AsNoTracking(), r => r.Moduloid, m => m.Moduloid, (r, m) => m)
            .OrderBy(m => m.Posicao)
            .ToList();

    public IEnumerable<Modulo> ListaPorUsuario(string usuarioid) =>
        _db.Relmodulousuarios.AsNoTracking()
            .Where(r => r.Usuarioid == usuarioid)
            .Join(_db.Modulos.AsNoTracking(), r => r.Moduloid, m => m.Moduloid, (r, m) => m)
            .OrderBy(m => m.Posicao)
            .ToList();
}
