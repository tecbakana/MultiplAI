using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class AreasRepositorio : BaseRepositorio, IAreasRepositorio
{
    public AreasRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Area>> ListaAsync(string? aplicacaoid) =>
        string.IsNullOrEmpty(aplicacaoid)
            ? await _db.Areas.AsNoTracking().OrderBy(a => a.Posicao).ToListAsync()
            : await _db.Areas.AsNoTracking().Where(a => a.Aplicacaoid == aplicacaoid).OrderBy(a => a.Posicao).ToListAsync();

    public async Task<Area?> BuscaPorIdAsync(string id) =>
        await _db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Areaid == id);

    public async Task<Area?> BuscaHomeAsync(string aplicacaoid) =>
        await _db.Areas.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Aplicacaoid == aplicacaoid && a.Tipo == "home");

    public async Task<bool> ExisteHomeAsync(string aplicacaoid, string? excluirAreaId = null) =>
        await _db.Areas.AnyAsync(a =>
            a.Aplicacaoid == aplicacaoid &&
            a.Tipo == "home" &&
            (excluirAreaId == null || a.Areaid != excluirAreaId));

    public async Task<string> CriarAsync(AreaInput input, string aplicacaoid)
    {
        var area = new Area
        {
            Areaid      = Guid.NewGuid().ToString(),
            Nome        = input.Nome,
            Url         = input.Url,
            Descricao   = input.Descricao,
            Areaidpai   = input.Areaidpai,
            Posicao     = input.Posicao,
            Tipoarea    = input.Tipoarea,
            Tipo        = input.Tipo,
            Aplicacaoid = aplicacaoid,
            CanonicalArea = input.CanonicalArea,
            Datainicial = DateTime.UtcNow,
            PageBuilderVersion = "area"
        };
        _db.Areas.Add(area);
        await _db.SaveChangesAsync();
        return area.Areaid;
    }

    public async Task<bool> AtualizarAsync(string id, AreaInput input)
    {
        var area = await _db.Areas.FirstOrDefaultAsync(a => a.Areaid == id);
        if (area == null) return false;
        area.Nome      = input.Nome;
        area.Url       = input.Url;
        area.Descricao = input.Descricao;
        area.Areaidpai = input.Areaidpai;
        area.Posicao   = input.Posicao;
        area.Tipoarea  = input.Tipoarea;
        area.Tipo      = input.Tipo;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AtualizarLayoutAsync(string id, string layout)
    {
        var area = await _db.Areas.FirstOrDefaultAsync(a => a.Areaid == id);
        if (area == null) return false;
        area.Layout = layout;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AtualizarPageBuilderVersionAsync(string id, string version)
    {
        var area = await _db.Areas.FirstOrDefaultAsync(a => a.Areaid == id);
        if (area == null) return false;
        area.PageBuilderVersion = version;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task RemoverAsync(Area area)
    {
        _db.Areas.Remove(area);
        await _db.SaveChangesAsync();
    }
}
