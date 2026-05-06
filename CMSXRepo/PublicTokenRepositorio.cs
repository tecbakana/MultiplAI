using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class PublicTokenRepositorio : BaseRepositorio, IPublicTokenRepositorio
{
    public PublicTokenRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<PublicToken> Lista(string aplicacaoid) =>
        _db.PublicTokens
            .AsNoTracking()
            .Where(t => t.Aplicacaoid == aplicacaoid)
            .OrderByDescending(t => t.Datainclusao)
            .ToList();

    public PublicToken? BuscaPorId(Guid id) =>
        _db.PublicTokens.FirstOrDefault(t => t.PublicTokenId == id);

    public void Criar(PublicToken token)
    {
        _db.PublicTokens.Add(token);
        _db.SaveChanges();
    }

    public void Revogar(PublicToken token)
    {
        token.Ativo = false;
        _db.SaveChanges();
    }
}
