using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CMSXRepo;

public class VitrineRepositorio : BaseRepositorio, IVitrineRepositorio
{
    public VitrineRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<VitrineTemplateResumo>> ListaTemplatesAsync() =>
        await _db.VitrineTemplates
            .AsNoTracking()
            .Where(t => t.Ativo)
            .OrderByDescending(t => t.DataCriacao)
            .Select(t => new VitrineTemplateResumo(
                t.VitrineTemplateId, t.Nome, t.Descricao,
                t.SegmentoTenantId, t.VariaveisJson, t.ThumbnailUrl, t.DataCriacao))
            .ToListAsync();

    public async ValueTask<VitrineTemplateDetalhe?> BuscaTemplateAsync(Guid id)
    {
        var t = await _db.VitrineTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.VitrineTemplateId == id && t.Ativo);
        return t is null ? null : new VitrineTemplateDetalhe(
            t.VitrineTemplateId, t.Nome, t.Descricao, t.SegmentoTenantId,
            t.HtmlCss, t.VariaveisJson, t.ThumbnailUrl, t.DataCriacao);
    }

    public async Task<Guid> CriarTemplateAsync(VitrineTemplateInput input)
    {
        var id = Guid.NewGuid();
        _db.VitrineTemplates.Add(new VitrineTemplate
        {
            VitrineTemplateId = id,
            Nome = input.Nome,
            Descricao = input.Descricao,
            SegmentoTenantId = input.SegmentoTenantId,
            HtmlCss = input.HtmlCss,
            VariaveisJson = input.VariaveisJson,
            ThumbnailUrl = input.ThumbnailUrl,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        });
        await _db.SaveChangesAsync();
        return id;
    }

    public async Task<bool> AtualizarTemplateAsync(Guid id, VitrineTemplateInput input)
    {
        var template = await _db.VitrineTemplates
            .FirstOrDefaultAsync(t => t.VitrineTemplateId == id && t.Ativo);
        if (template is null) return false;

        template.Nome = input.Nome;
        template.Descricao = input.Descricao;
        template.SegmentoTenantId = input.SegmentoTenantId;
        template.HtmlCss = input.HtmlCss;
        template.VariaveisJson = input.VariaveisJson;
        template.ThumbnailUrl = input.ThumbnailUrl;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesativarTemplateAsync(Guid id)
    {
        var template = await _db.VitrineTemplates
            .FirstOrDefaultAsync(t => t.VitrineTemplateId == id);
        if (template is null) return false;
        template.Ativo = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<VitrineTemplateResumo>> ListaTemplatesDisponiveisAsync(string aplicacaoId)
    {
        var segmentos = await _db.AplicacaoSegmentos
            .AsNoTracking()
            .Where(s => s.AplicacaoId == aplicacaoId)
            .Select(s => s.SegmentoTenantId)
            .ToListAsync();

        return await _db.VitrineTemplates
            .AsNoTracking()
            .Where(t => t.Ativo && (t.SegmentoTenantId == null || segmentos.Contains(t.SegmentoTenantId)))
            .OrderByDescending(t => t.DataCriacao)
            .Select(t => new VitrineTemplateResumo(
                t.VitrineTemplateId, t.Nome, t.Descricao,
                t.SegmentoTenantId, t.VariaveisJson, t.ThumbnailUrl, t.DataCriacao))
            .ToListAsync();
    }

    public async ValueTask<VitrineConfiguradaResumo?> BuscaConfiguradaAsync(string aplicacaoId)
    {
        var vc = await _db.VitrineConfiguradas
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.AplicacaoId == aplicacaoId);
        return vc is null ? null : new VitrineConfiguradaResumo(
            vc.VitrineConfiguradaId, vc.AplicacaoId, vc.VitrineTemplateId,
            vc.ValoresJson, vc.Publicado);
    }

    public async Task<Guid> SalvarConfiguradaAsync(string aplicacaoId, VitrineConfiguradaInput input)
    {
        var existente = await _db.VitrineConfiguradas
            .FirstOrDefaultAsync(v => v.AplicacaoId == aplicacaoId);

        if (existente is null)
        {
            var id = Guid.NewGuid();
            _db.VitrineConfiguradas.Add(new VitrineConfigurada
            {
                VitrineConfiguradaId = id,
                AplicacaoId = aplicacaoId,
                VitrineTemplateId = input.VitrineTemplateId,
                ValoresJson = input.ValoresJson,
                Publicado = false,
                DataAtualizacao = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return id;
        }

        existente.VitrineTemplateId = input.VitrineTemplateId;
        existente.ValoresJson = input.ValoresJson;
        existente.DataAtualizacao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return existente.VitrineConfiguradaId;
    }

    public async Task<bool> PublicarAsync(string aplicacaoId, string htmlSnapshot)
    {
        var vc = await _db.VitrineConfiguradas
            .FirstOrDefaultAsync(v => v.AplicacaoId == aplicacaoId);
        if (vc is null) return false;

        // Extrai CSS inline para armazenamento separado
        var cssMatch = Regex.Match(htmlSnapshot, @"<style[^>]*>([\s\S]*?)</style>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var htmlSemCss = htmlSnapshot;
        string? css = null;
        if (cssMatch.Success)
        {
            css = cssMatch.Groups[1].Value;
            var v = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var linkTag = $"<link rel=\"stylesheet\" href=\"/vitrine/css/{vc.VitrineConfiguradaId}?v={v}\">";
            htmlSemCss = htmlSnapshot.Remove(cssMatch.Index, cssMatch.Length).Insert(cssMatch.Index, linkTag);
        }

        vc.Publicado = true;
        vc.HtmlSnapshot = htmlSemCss;
        vc.CssProcessado = css;
        vc.DataAtualizacao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string?> BuscaSnapshotAsync(string aplicacaoId) =>
        await _db.VitrineConfiguradas
            .AsNoTracking()
            .Where(v => v.AplicacaoId == aplicacaoId && v.Publicado)
            .Select(v => v.HtmlSnapshot)
            .FirstOrDefaultAsync();

    public async Task<string?> BuscaCssProcessadoAsync(Guid vitrineConfiguradaId) =>
        await _db.VitrineConfiguradas
            .AsNoTracking()
            .Where(v => v.VitrineConfiguradaId == vitrineConfiguradaId && v.Publicado)
            .Select(v => v.CssProcessado)
            .FirstOrDefaultAsync();

    public async Task<string?> RenderAsync(string aplicacaoId, string? conteudoPb = null, string? navHtml = null)
    {
        var vc = await _db.VitrineConfiguradas
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.AplicacaoId == aplicacaoId);
        if (vc is null) return null;

        var template = await _db.VitrineTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.VitrineTemplateId == vc.VitrineTemplateId && t.Ativo);
        if (template is null) return null;

        var resultado = AplicarValores(template.HtmlCss, vc.ValoresJson);

        if (!string.IsNullOrEmpty(navHtml))
        {
            resultado = Regex.Replace(
                resultado,
                @"(<[^>]+data-vitrine-slot=""nav""[^>]*>)(.*?)(</[^>]+>)",
                m => m.Groups[1].Value + navHtml + m.Groups[3].Value,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        if (string.IsNullOrEmpty(conteudoPb))
            return resultado;

        // Tenta injetar dentro do slot declarado
        var comSlot = Regex.Replace(
            resultado,
            @"(<[^>]+data-vitrine-slot=""conteudo""[^>]*>)(.*?)(</[^>]+>)",
            m => m.Groups[1].Value + conteudoPb + m.Groups[3].Value,
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Se o regex substituiu algo, o HTML mudou; caso contrário, appenda antes de </body>
        if (!ReferenceEquals(comSlot, resultado) && comSlot != resultado)
            return comSlot;

        var bodyClose = resultado.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        return bodyClose >= 0
            ? resultado.Insert(bodyClose, conteudoPb)
            : resultado + conteudoPb;
    }

    public async Task<string?> RenderComDadosAsync(string aplicacaoId, ISiteRepositorio siteRepo,
        string? navHtml = null, string? conteudoPb = null)
    {
        var html = await RenderAsync(aplicacaoId, conteudoPb, navHtml);
        if (html is null) return null;

        var vc = await _db.VitrineConfiguradas
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.AplicacaoId == aplicacaoId);

        Dictionary<string, string?> valores;
        try { valores = JsonSerializer.Deserialize<Dictionary<string, string?>>(vc?.ValoresJson ?? "{}") ?? []; }
        catch { valores = []; }

        var matches = Regex.Matches(html,
            @"(<[^>]*data-vitrine-bloco=""([\w-]+)""[^>]*>)([\s\S]*?)(</[\w]+>)",
            RegexOptions.IgnoreCase).Cast<Match>().ToList();

        var resultado = html;
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var m = matches[i];
            var tipoBloco = m.Groups[2].Value;
            var innerGroup = m.Groups[3];
            var inner = await GerarHtmlBlocoAsync(tipoBloco, valores, aplicacaoId, siteRepo);
            if (inner is null) continue;
            resultado = resultado[..innerGroup.Index] + inner + resultado[(innerGroup.Index + innerGroup.Length)..];
        }

        return resultado;
    }

    private static async Task<string?> GerarHtmlBlocoAsync(string tipoBloco,
        Dictionary<string, string?> valores, string aplicacaoId, ISiteRepositorio siteRepo)
    {
        var limiteStr = valores.GetValueOrDefault($"{tipoBloco}__limite");
        var limite = int.TryParse(limiteStr, out var l) ? l : 8;

        return tipoBloco switch
        {
            "lista-produtos" => await GerarHtmlProdutosAsync(
                aplicacaoId, valores.GetValueOrDefault($"{tipoBloco}__cateriaid"), limite, siteRepo),
            "lista-conteudos" => await GerarHtmlConteudosAsync(
                valores.GetValueOrDefault($"{tipoBloco}__areaid") ?? "", limite, siteRepo),
            "lista-categorias" => await GerarHtmlCategoriasAsync(
                aplicacaoId, valores.GetValueOrDefault($"{tipoBloco}__cateriaidpai"), siteRepo),
            _ => null
        };
    }

    private static async Task<string> GerarHtmlProdutosAsync(
        string aplicacaoId, string? cateriaid, int limite, ISiteRepositorio siteRepo)
    {
        var produtos = await siteRepo.ListaProdutosAsync(
            aplicacaoId, string.IsNullOrEmpty(cateriaid) ? null : cateriaid, limite);
        var sb = new StringBuilder();
        const string svgPlaceholder = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='150'%3E%3Crect width='200' height='150' fill='%23f3f4f6' rx='4'/%3E%3C/svg%3E";
        foreach (var p in produtos)
        {
            var src = string.IsNullOrEmpty(p.Imagem) ? svgPlaceholder : p.Imagem;
            sb.Append($"<div class=\"vitrine-produto\"><img src=\"{src}\" alt=\"{WebUtility.HtmlEncode(p.Nome ?? "")}\" class=\"vitrine-produto-img\"><h3 class=\"vitrine-produto-nome\">{WebUtility.HtmlEncode(p.Nome ?? "")}</h3><p class=\"vitrine-produto-desc\">{WebUtility.HtmlEncode(p.Descricacurta ?? "")}</p><span class=\"vitrine-produto-preco\">R$ {p.Valor?.ToString("F2") ?? "—"}</span></div>");
        }
        return sb.ToString();
    }

    private static async Task<string> GerarHtmlConteudosAsync(
        string areaid, int limite, ISiteRepositorio siteRepo)
    {
        if (string.IsNullOrEmpty(areaid)) return "";
        var conteudos = await siteRepo.ListaConteudosPorAreaAsync(areaid, limite);
        var sb = new StringBuilder();
        foreach (var c in conteudos)
        {
            var texto = c.Texto ?? "";
            var resumo = texto.Length > 120 ? texto[..120] + "…" : texto;
            sb.Append($"<article class=\"vitrine-conteudo\"><h3 class=\"vitrine-conteudo-titulo\">{WebUtility.HtmlEncode(c.Titulo ?? "")}</h3><p class=\"vitrine-conteudo-resumo\">{WebUtility.HtmlEncode(resumo)}</p><time class=\"vitrine-conteudo-data\">{c.Datainclusao:d}</time></article>");
        }
        return sb.ToString();
    }

    private static async Task<string> GerarHtmlCategoriasAsync(
        string aplicacaoId, string? cateriaidpai, ISiteRepositorio siteRepo)
    {
        var cats = await siteRepo.ListaCategoriasAsync(
            aplicacaoId, string.IsNullOrEmpty(cateriaidpai) ? null : cateriaidpai);
        var sb = new StringBuilder();
        foreach (var cat in cats)
            sb.Append($"<a class=\"vitrine-categoria\" href=\"/s/{aplicacaoId}/{cat.Cateriaid}\">{WebUtility.HtmlEncode(cat.Nome ?? "")}</a>");
        return sb.ToString();
    }

    private static string AplicarValores(string htmlCss, string valoresJson)
    {
        Dictionary<string, string?> valores;
        try { valores = JsonSerializer.Deserialize<Dictionary<string, string?>>(valoresJson) ?? []; }
        catch { return htmlCss; }

        var resultado = htmlCss;

        var cssVars = valores.Where(kvp => kvp.Key.StartsWith("--")).ToList();
        if (cssVars.Count > 0)
        {
            var injecao = string.Join("; ", cssVars.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            resultado = Regex.Replace(resultado, @":root\s*\{", $":root {{ {injecao}; ", RegexOptions.IgnoreCase);
            if (!resultado.Contains(":root"))
                resultado = $"<style>:root {{ {injecao}; }}</style>" + resultado;
        }

        foreach (var (chave, valor) in valores.Where(kvp => !kvp.Key.StartsWith("--")))
        {
            if (valor is null) continue;

            var valorTexto = HtmlEncoder.Default.Encode(valor);
            resultado = Regex.Replace(
                resultado,
                $@"(data-vitrine-texto=""{Regex.Escape(chave)}""[^>]*)>([^<]*)<",
                $@"$1>{valorTexto}<",
                RegexOptions.IgnoreCase);

            if (Uri.TryCreate(valor, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                resultado = Regex.Replace(
                    resultado,
                    $@"(data-vitrine-imagem=""{Regex.Escape(chave)}"")([^>]*)(src=""[^""]*"")?",
                    $@"$1$2 src=""{valor}""",
                    RegexOptions.IgnoreCase);
            }
        }

        return resultado;
    }
}
