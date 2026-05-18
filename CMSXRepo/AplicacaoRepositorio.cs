using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class AplicacaoRepositorio : BaseRepositorio, IAplicacaoRepositorio
{
    public AplicacaoRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Aplicacao>> ListaAsync(string? aplicacaoid) =>
        string.IsNullOrEmpty(aplicacaoid)
            ? await _db.Aplicacaos.AsNoTracking().OrderBy(a => a.Nome).ToListAsync()
            : await _db.Aplicacaos.AsNoTracking().Where(a => a.Aplicacaoid == aplicacaoid).ToListAsync();

    public async Task<Aplicacao?> BuscaPorIdAsync(string id) =>
        await _db.Aplicacaos.AsNoTracking().FirstOrDefaultAsync(a => a.Aplicacaoid == id);

    public async Task<LayoutTemplate?> BuscaTemplatePadraoAsync() =>
        await _db.LayoutTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Tipo == "home" && t.Padrao);

    public async Task CriarAsync(Aplicacao aplicacao, Area homeArea)
    {
        _db.Aplicacaos.Add(aplicacao);
        _db.Areas.Add(homeArea);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Aplicacao aplicacao)
    {
        _db.Aplicacaos.Update(aplicacao);
        await _db.SaveChangesAsync();
    }

    public async Task AlterarStatusAsync(Aplicacao aplicacao, bool ativo)
    {
        aplicacao.Isactive = ativo;
        aplicacao.Datafinal = ativo ? null : DateTime.UtcNow;
        _db.Aplicacaos.Update(aplicacao);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Aplicacao aplicacao)
    {
        _db.Aplicacaos.Remove(aplicacao);
        await _db.SaveChangesAsync();
    }

    public async Task SalvarLogoAsync(string aplicacaoId, byte[] bytes, string contentType)
    {
        var app = await _db.Aplicacaos.FirstOrDefaultAsync(a => a.Aplicacaoid == aplicacaoId);
        if (app is null) return;
        app.Lotipo = bytes;
        app.LogoContentType = contentType;
        await _db.SaveChangesAsync();
    }

    public async Task<(byte[]? Bytes, string? ContentType)> BuscaLogoAsync(string aplicacaoId)
    {
        var app = await _db.Aplicacaos
            .AsNoTracking()
            .Where(a => a.Aplicacaoid == aplicacaoId)
            .Select(a => new { a.Lotipo, a.LogoContentType })
            .FirstOrDefaultAsync();
        return (app?.Lotipo, app?.LogoContentType);
    }
}
