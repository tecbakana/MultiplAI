using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class AplicacaoRepositorio : BaseRepositorio, IAplicacaoRepositorio
{
    public AplicacaoRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Aplicacao> Lista(string? aplicacaoid) =>
        string.IsNullOrEmpty(aplicacaoid)
            ? _db.Aplicacaos.AsNoTracking().OrderBy(a => a.Nome).ToList()
            : _db.Aplicacaos.AsNoTracking().Where(a => a.Aplicacaoid == aplicacaoid).ToList();

    public Aplicacao? BuscaPorId(string id) =>
        _db.Aplicacaos.AsNoTracking().FirstOrDefault(a => a.Aplicacaoid == id);

    public LayoutTemplate? BuscaTemplatePadrao() =>
        _db.LayoutTemplates.AsNoTracking().FirstOrDefault(t => t.Tipo == "home" && t.Padrao);

    public void Criar(Aplicacao aplicacao, Area homeArea)
    {
        _db.Aplicacaos.Add(aplicacao);
        _db.Areas.Add(homeArea);
        _db.SaveChanges();
    }

    public void Atualizar(Aplicacao aplicacao)
    {
        _db.Aplicacaos.Update(aplicacao);
        _db.SaveChanges();
    }

    public void AlterarStatus(Aplicacao aplicacao, bool ativo)
    {
        aplicacao.Isactive = ativo;
        aplicacao.Datafinal = ativo ? null : DateTime.UtcNow;
        _db.Aplicacaos.Update(aplicacao);
        _db.SaveChanges();
    }

    public void Remover(Aplicacao aplicacao)
    {
        _db.Aplicacaos.Remove(aplicacao);
        _db.SaveChanges();
    }
}
