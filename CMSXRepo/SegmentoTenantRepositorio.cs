using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class SegmentoTenantRepositorio : BaseRepositorio, ISegmentoTenantRepositorio
{
    public SegmentoTenantRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<SegmentoTenantResultado>> ListaAtivosAsync() =>
        await _db.SegmentoTenants
            .AsNoTracking()
            .Where(s => s.Ativo)
            .OrderBy(s => s.Nome)
            .Select(s => new SegmentoTenantResultado(s.SegmentoTenantId, s.Nome, s.Descricao, s.Ativo))
            .ToListAsync();

    public async Task<SegmentoTenantResultado?> BuscaPorIdAsync(string id)
    {
        var s = await _db.SegmentoTenants.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SegmentoTenantId == id);
        return s == null ? null : new SegmentoTenantResultado(s.SegmentoTenantId, s.Nome, s.Descricao, s.Ativo);
    }

    public async Task<string> CriarAsync(SegmentoTenantInput input)
    {
        var segmento = new SegmentoTenant
        {
            SegmentoTenantId = Guid.NewGuid().ToString(),
            Nome             = input.Nome,
            Descricao        = input.Descricao,
            Ativo            = true
        };
        _db.SegmentoTenants.Add(segmento);
        await _db.SaveChangesAsync();
        return segmento.SegmentoTenantId;
    }

    public async Task<bool> AtualizarAsync(string id, SegmentoTenantInput input)
    {
        var segmento = await _db.SegmentoTenants.FirstOrDefaultAsync(s => s.SegmentoTenantId == id);
        if (segmento == null) return false;
        segmento.Nome     = input.Nome;
        segmento.Descricao = input.Descricao;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoverAsync(string id)
    {
        var segmento = await _db.SegmentoTenants.FirstOrDefaultAsync(s => s.SegmentoTenantId == id);
        if (segmento == null) return false;
        segmento.Ativo = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SegmentoTenantResultado>> ListaPorAplicacaoAsync(string aplicacaoid) =>
        await (from asg in _db.AplicacaoSegmentos.AsNoTracking()
               join seg in _db.SegmentoTenants.AsNoTracking()
                   on asg.SegmentoTenantId equals seg.SegmentoTenantId
               where asg.AplicacaoId == aplicacaoid && seg.Ativo
               orderby seg.Nome
               select new SegmentoTenantResultado(seg.SegmentoTenantId, seg.Nome, seg.Descricao, seg.Ativo))
              .ToListAsync();

    public async Task VincularAsync(string aplicacaoid, string segmentoTenantId)
    {
        var existe = await _db.AplicacaoSegmentos
            .AnyAsync(a => a.AplicacaoId == aplicacaoid && a.SegmentoTenantId == segmentoTenantId);
        if (existe) return;
        _db.AplicacaoSegmentos.Add(new AplicacaoSegmento
        {
            AplicacaoId      = aplicacaoid,
            SegmentoTenantId = segmentoTenantId
        });
        await _db.SaveChangesAsync();
    }

    public async Task DesvincularAsync(string aplicacaoid, string segmentoTenantId)
    {
        var vinculo = await _db.AplicacaoSegmentos
            .FirstOrDefaultAsync(a => a.AplicacaoId == aplicacaoid && a.SegmentoTenantId == segmentoTenantId);
        if (vinculo == null) return;
        _db.AplicacaoSegmentos.Remove(vinculo);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> ListaSegmentoIdsPorAplicacaoAsync(string aplicacaoid) =>
        await _db.AplicacaoSegmentos
            .AsNoTracking()
            .Where(a => a.AplicacaoId == aplicacaoid)
            .Select(a => a.SegmentoTenantId)
            .ToListAsync();
}
