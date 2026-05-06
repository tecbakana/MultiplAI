using CMSXData.Models;
using ICMSX;

namespace CMSXRepo;

public class DashboardRepositorio : BaseRepositorio, IDashboardRepositorio
{
    public DashboardRepositorio(CmsxDbContext db) : base(db) { }

    public DashboardTotais TotaisGlobais() =>
        new(
            Usuarios:   _db.Usuarios.Count(),
            Aplicacoes: _db.Aplicacaos.Count(),
            Conteudos:  _db.Conteudos.Count(),
            Areas:      _db.Areas.Count(),
            Categorias: _db.Cateria.Count(),
            Modulos:    _db.Modulos.Count()
        );

    public DashboardTotais TotaisPorAplicacao(string aplicacaoid)
    {
        var areaIds = _db.Areas
            .Where(a => a.Aplicacaoid == aplicacaoid)
            .Select(a => a.Areaid)
            .ToHashSet();

        return new(
            Usuarios:   _db.Relusuarioaplicacaos.Count(r => r.Aplicacaoid == aplicacaoid),
            Aplicacoes: _db.Aplicacaos.Count(a => a.Aplicacaoid == aplicacaoid),
            Conteudos:  _db.Conteudos.Count(c => c.Areaid != null && areaIds.Contains(c.Areaid)),
            Areas:      areaIds.Count,
            Categorias: _db.Cateria.Count(c => c.Aplicacaoid == aplicacaoid),
            Modulos:    _db.Relmoduloaplicacaos.Count(r => r.Aplicacaoid == aplicacaoid)
        );
    }
}
