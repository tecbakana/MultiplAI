using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class PublicTokenRepositorio : BaseRepositorio, IPublicTokenRepositorio
{
    public PublicTokenRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<PublicToken>> ListaAsync(string aplicacaoid) =>
        await _db.PublicTokens
            .AsNoTracking()
            .Where(t => t.Aplicacaoid == aplicacaoid)
            .OrderByDescending(t => t.Datainclusao)
            .ToListAsync();

    public async Task<PublicToken?> BuscaPorIdAsync(Guid id) =>
        await _db.PublicTokens.FirstOrDefaultAsync(t => t.PublicTokenId == id);

    public async Task<PublicToken?> BuscaPorAplicacaoAsync(string aplicacaoid) =>
        await _db.PublicTokens
            .AsNoTracking()
            .Where(t => t.Aplicacaoid == aplicacaoid && t.Ativo &&
                (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow))
            .OrderByDescending(t => t.Datainclusao)
            .FirstOrDefaultAsync();

    public async Task<string?> ResolveAsync(string token) =>
        await _db.PublicTokens
            .AsNoTracking()
            .Where(t => t.Token == token && t.Ativo &&
                (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow))
            .Select(t => t.Aplicacaoid)
            .FirstOrDefaultAsync();

    public async Task CriarAsync(PublicToken token)
    {
        _db.PublicTokens.Add(token);
        await _db.SaveChangesAsync();
    }

    public async Task RevogarAsync(PublicToken token)
    {
        token.Ativo = false;
        await _db.SaveChangesAsync();
    }
}
