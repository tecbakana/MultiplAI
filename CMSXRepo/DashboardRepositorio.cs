using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class DashboardRepositorio : BaseRepositorio, IDashboardRepositorio
{
    public DashboardRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<DashboardTotais> TotaisGlobaisAsync() =>
        new(
            Usuarios:   await _db.Usuarios.CountAsync(),
            Aplicacoes: await _db.Aplicacaos.CountAsync(),
            Conteudos:  await _db.Conteudos.CountAsync(),
            Areas:      await _db.Areas.CountAsync(),
            Categorias: await _db.Cateria.CountAsync(),
            Modulos:    await _db.Modulos.CountAsync()
        );

    public async Task<DashboardTotais> TotaisPorAplicacaoAsync(string aplicacaoid)
    {
        var areaIds = await _db.Areas
            .Where(a => a.Aplicacaoid == aplicacaoid)
            .Select(a => a.Areaid)
            .ToListAsync();

        return new(
            Usuarios:   await _db.Relusuarioaplicacaos.CountAsync(r => r.Aplicacaoid == aplicacaoid),
            Aplicacoes: await _db.Aplicacaos.CountAsync(a => a.Aplicacaoid == aplicacaoid),
            Conteudos:  await _db.Conteudos.CountAsync(c => c.Areaid != null && areaIds.Contains(c.Areaid)),
            Areas:      areaIds.Count,
            Categorias: await _db.Cateria.CountAsync(c => c.Aplicacaoid == aplicacaoid),
            Modulos:    await _db.Relmoduloaplicacaos.CountAsync(r => r.Aplicacaoid == aplicacaoid)
        );
    }
}
