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

    public async Task<Modulo?> BuscaPorIdAsync(string moduloid) =>
        await _db.Modulos.AsNoTracking().FirstOrDefaultAsync(m => m.Moduloid == moduloid);

    public async Task<string> CriarAsync(ModuloInput input)
    {
        var modulo = new Modulo
        {
            Moduloid = Guid.NewGuid().ToString(),
            Nome = input.Nome,
            Url = input.Url,
            Posicao = input.Posicao
        };
        _db.Modulos.Add(modulo);
        await _db.SaveChangesAsync();
        return modulo.Moduloid;
    }

    public async Task<bool> AtualizarAsync(string id, ModuloInput input)
    {
        var modulo = await _db.Modulos.FirstOrDefaultAsync(m => m.Moduloid == id);
        if (modulo == null) return false;

        modulo.Nome = input.Nome;
        modulo.Url = input.Url;
        modulo.Posicao = input.Posicao;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoverAsync(string id)
    {
        var modulo = await _db.Modulos.FirstOrDefaultAsync(m => m.Moduloid == id);
        if (modulo == null) return false;

        _db.Modulos.Remove(modulo);
        await _db.SaveChangesAsync();
        return true;
    }
}
