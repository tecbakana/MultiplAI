using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ModuloRepositorio : BaseRepositorio, IModuloRepositorio
{
    public ModuloRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Modulo>> ListaTodosAsync() =>
        await _db.Modulos.AsNoTracking().OrderBy(m => m.Posicao).ToListAsync();

    public async Task<IEnumerable<Modulo>> ListaPorAplicacaoAsync(string aplicacaoid) =>
        await _db.Relmoduloaplicacaos.AsNoTracking()
            .Where(r => r.Aplicacaoid == aplicacaoid)
            .Join(_db.Modulos.AsNoTracking(), r => r.Moduloid, m => m.Moduloid, (r, m) => m)
            .OrderBy(m => m.Posicao)
            .ToListAsync();

    public async Task<IEnumerable<Modulo>> ListaPorUsuarioAsync(string usuarioid) =>
        await _db.Relmodulousuarios.AsNoTracking()
            .Where(r => r.Usuarioid == usuarioid)
            .Join(_db.Modulos.AsNoTracking(), r => r.Moduloid, m => m.Moduloid, (r, m) => m)
            .OrderBy(m => m.Posicao)
            .ToListAsync();
}
