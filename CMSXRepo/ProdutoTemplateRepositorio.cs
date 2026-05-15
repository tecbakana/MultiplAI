using System.Text.Json;
using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ProdutoTemplateRepositorio : BaseRepositorio, IProdutoTemplateRepositorio
{
    public ProdutoTemplateRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<ProdutoTemplateResultado>> ListaPorAplicacaoAsync(string aplicacaoid) =>
        await _db.ProdutoTemplates
            .AsNoTracking()
            .Where(t => t.Aplicacaoid == aplicacaoid && t.Ativo)
            .OrderByDescending(t => t.DataCriacao)
            .Select(t => new ProdutoTemplateResultado(
                t.ProdutoTemplateid, t.Aplicacaoid, t.SegmentoTenantId,
                t.Nome, t.Descricao, t.ConteudoJson, t.DataCriacao))
            .ToListAsync();

    public async Task<ProdutoTemplateResultado?> BuscaPorIdAsync(string id, string aplicacaoid)
    {
        var t = await _db.ProdutoTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ProdutoTemplateid == id && t.Aplicacaoid == aplicacaoid && t.Ativo);
        return t == null ? null : new ProdutoTemplateResultado(
            t.ProdutoTemplateid, t.Aplicacaoid, t.SegmentoTenantId,
            t.Nome, t.Descricao, t.ConteudoJson, t.DataCriacao);
    }

    public async Task<ProdutoTemplateResultado?> BuscaAcessivelAsync(
        string id, string aplicacaoid, IEnumerable<string> segmentoIds)
    {
        var segList = segmentoIds.ToList();
        var t = await _db.ProdutoTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ProdutoTemplateid == id && t.Ativo &&
                (t.Aplicacaoid == aplicacaoid ||
                 (t.SegmentoTenantId != null && segList.Contains(t.SegmentoTenantId))));
        return t == null ? null : new ProdutoTemplateResultado(
            t.ProdutoTemplateid, t.Aplicacaoid, t.SegmentoTenantId,
            t.Nome, t.Descricao, t.ConteudoJson, t.DataCriacao);
    }

    public async Task<string> CriarAsync(ProdutoTemplateInput input, string aplicacaoid)
    {
        var template = new ProdutoTemplate
        {
            ProdutoTemplateid = Guid.NewGuid().ToString(),
            Aplicacaoid       = aplicacaoid,
            Nome              = input.Nome,
            Descricao         = input.Descricao,
            ConteudoJson      = input.ConteudoJson,
            DataCriacao       = DateTime.UtcNow,
            Ativo             = true
        };
        _db.ProdutoTemplates.Add(template);
        await _db.SaveChangesAsync();
        return template.ProdutoTemplateid;
    }

    public async Task<bool> RemoverAsync(string id, string aplicacaoid)
    {
        var template = await _db.ProdutoTemplates
            .FirstOrDefaultAsync(t => t.ProdutoTemplateid == id && t.Aplicacaoid == aplicacaoid && t.Ativo);
        if (template == null) return false;
        template.Ativo = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProdutoTemplateResultado>> ListaComSegmentosAsync(
        string aplicacaoid, IEnumerable<string> segmentoIds)
    {
        var segList = segmentoIds.ToList();
        return await _db.ProdutoTemplates
            .AsNoTracking()
            .Where(t => t.Ativo &&
                ((t.Aplicacaoid == aplicacaoid && t.SegmentoTenantId == null) ||
                 (t.SegmentoTenantId != null && segList.Contains(t.SegmentoTenantId))))
            .OrderByDescending(t => t.DataCriacao)
            .Select(t => new ProdutoTemplateResultado(
                t.ProdutoTemplateid, t.Aplicacaoid, t.SegmentoTenantId,
                t.Nome, t.Descricao, t.ConteudoJson, t.DataCriacao))
            .ToListAsync();
    }

    public async Task<IEnumerable<ProdutoTemplateResultado>> ListaPorSegmentoAsync(string segmentoTenantId) =>
        await _db.ProdutoTemplates
            .AsNoTracking()
            .Where(t => t.Ativo && t.SegmentoTenantId == segmentoTenantId)
            .OrderByDescending(t => t.DataCriacao)
            .Select(t => new ProdutoTemplateResultado(
                t.ProdutoTemplateid, t.Aplicacaoid, t.SegmentoTenantId,
                t.Nome, t.Descricao, t.ConteudoJson, t.DataCriacao))
            .ToListAsync();

    public async Task<string> CriarSegmentoAsync(ProdutoTemplateInput input, string segmentoTenantId)
    {
        var template = new ProdutoTemplate
        {
            ProdutoTemplateid = Guid.NewGuid().ToString(),
            Aplicacaoid       = "",
            SegmentoTenantId  = segmentoTenantId,
            Nome              = input.Nome,
            Descricao         = input.Descricao,
            ConteudoJson      = input.ConteudoJson,
            DataCriacao       = DateTime.UtcNow,
            Ativo             = true
        };
        _db.ProdutoTemplates.Add(template);
        await _db.SaveChangesAsync();
        return template.ProdutoTemplateid;
    }

    public async Task<bool> AplicarTemplateAsync(string produtoId, string conteudoJson)
    {
        using var doc = JsonDocument.Parse(conteudoJson);
        var root = doc.RootElement;
        if (!root.TryGetProperty("atributos", out var atributosEl) || atributosEl.ValueKind != JsonValueKind.Array)
            return false;
        await CriarAtributosRecursivamenteAsync(produtoId, atributosEl, null);
        return true;
    }

    private async Task CriarAtributosRecursivamenteAsync(string produtoId, JsonElement atributosEl, Guid? parentId)
    {
        int ordem = 0;
        foreach (var el in atributosEl.EnumerateArray())
        {
            var atributo = new Atributo
            {
                Atributoid       = Guid.NewGuid(),
                Produtoid        = parentId.HasValue ? null : produtoId,
                ParentAtributoId = parentId,
                Nome             = el.TryGetProperty("nome", out var n) ? n.GetString() ?? "" : "",
                Descricao        = el.TryGetProperty("descricao", out var d) ? d.GetString() ?? "" : "",
                Ordem            = el.TryGetProperty("ordem", out var o) && o.ValueKind == JsonValueKind.Number
                                    ? o.GetInt32() : ++ordem,
                ValorAdicional   = el.TryGetProperty("valorAdicional", out var va) && va.ValueKind == JsonValueKind.Number
                                    ? va.GetDecimal() : null
            };
            _db.Atributos.Add(atributo);
            await _db.SaveChangesAsync();

            if (el.TryGetProperty("opcoes", out var opcoesEl) && opcoesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var opcaoEl in opcoesEl.EnumerateArray())
                {
                    var nomeOpcao = opcaoEl.ValueKind == JsonValueKind.String
                        ? opcaoEl.GetString() ?? ""
                        : opcaoEl.TryGetProperty("nome", out var on) ? on.GetString() ?? "" : "";
                    _db.Opcaos.Add(new Opcao
                    {
                        Opcaoid    = Guid.NewGuid().ToString(),
                        Atributoid = atributo.Atributoid,
                        Nome       = nomeOpcao,
                        Qtd        = 1
                    });
                }
                await _db.SaveChangesAsync();
            }

            if (el.TryGetProperty("filhos", out var filhosEl) && filhosEl.ValueKind == JsonValueKind.Array
                && filhosEl.GetArrayLength() > 0)
                await CriarAtributosRecursivamenteAsync(produtoId, filhosEl, atributo.Atributoid);
        }
    }
}
