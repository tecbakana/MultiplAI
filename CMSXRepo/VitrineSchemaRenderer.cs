using ICMSX;
using System.Net;
using System.Text;

namespace CMSXRepo;

internal static class VitrineSchemaRenderer
{
    public static async Task<string> RenderAsync(
        VitrineConfig config, string aplicacaoId, ISiteRepositorio siteRepo,
        string? cssContent = null, string? logoSrc = null, string? slug = null)
    {
        var sb = new StringBuilder();
        sb.Append(GerarHead(config.Tema, cssContent));
        sb.Append("<body>");
        sb.Append(await GerarNavAsync(aplicacaoId, siteRepo, logoSrc, slug));
        sb.Append("<main>");
        foreach (var secao in config.Secoes)
            sb.Append(await GerarSecaoAsync(secao, aplicacaoId, siteRepo));
        sb.Append("</main>");
        sb.Append(GerarRodape());
        sb.Append("</body></html>");
        return sb.ToString();
    }

    // ── Head ──────────────────────────────────────────────────────────────

    private static string GerarHead(TemaConfig tema, string? cssContent)
    {
        var fontLink = GerarFontLink(tema);
        var vars = GerarCssVars(tema);
        var cssTag = cssContent is not null
            ? $"<style>{cssContent}</style>"
            : "<link rel=\"stylesheet\" href=\"/vitrine/design-system.css\">";
        return "<!DOCTYPE html><html lang=\"pt-BR\"><head>" +
               "<meta charset=\"UTF-8\">" +
               "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
               cssTag +
               fontLink +
               $"<style>:root{{{vars}}}</style>" +
               "</head>";
    }

    private static string GerarFontLink(TemaConfig tema)
    {
        var families = new HashSet<string>();
        var titulo = MapFontFamily(tema.FonteTitulo);
        var corpo  = MapFontFamily(tema.FonteCorpo);
        if (titulo is not null) families.Add(titulo);
        if (corpo  is not null) families.Add(corpo);
        if (families.Count == 0) return "";

        var query = string.Join("&family=", families);
        return $"<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">" +
               $"<link href=\"https://fonts.googleapis.com/css2?family={query}&display=swap\" rel=\"stylesheet\">";
    }

    private static string? MapFontFamily(string? fonte) => fonte switch
    {
        "serif"    => "Lora:wght@400;700",
        "moderna"  => "Poppins:wght@400;600;700",
        "classica" => "Playfair+Display:wght@400;700",
        _          => null
    };

    private static string GerarCssVars(TemaConfig tema)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(tema.CorPrimaria))   sb.Append($"--v-cor-primaria:{tema.CorPrimaria};");
        if (!string.IsNullOrEmpty(tema.CorSecundaria)) sb.Append($"--v-cor-secundaria:{tema.CorSecundaria};");
        if (!string.IsNullOrEmpty(tema.CorFundo))      sb.Append($"--v-cor-fundo:{tema.CorFundo};");
        if (!string.IsNullOrEmpty(tema.CorTexto))      sb.Append($"--v-cor-texto:{tema.CorTexto};");

        var fonteTitulo = MapFontValue(tema.FonteTitulo);
        var fonteCorpo  = MapFontValue(tema.FonteCorpo);
        if (fonteTitulo is not null) sb.Append($"--v-fonte-titulo:{fonteTitulo};");
        if (fonteCorpo  is not null) sb.Append($"--v-fonte-corpo:{fonteCorpo};");

        sb.Append(tema.Espacamento switch
        {
            "compacto" => "--v-gap:var(--v-gap-compacto);",
            "generoso" => "--v-gap:var(--v-gap-generoso);",
            _          => "--v-gap:var(--v-gap-confortavel);"
        });

        sb.Append(tema.RaioBorda switch
        {
            "nenhum"      => "--v-raio:var(--v-raio-nenhum);",
            "arredondado" => "--v-raio:var(--v-raio-arredondado);",
            _             => "--v-raio:var(--v-raio-suave);"
        });

        return sb.ToString();
    }

    private static string? MapFontValue(string? fonte) => fonte switch
    {
        "sans"     => "'Inter',system-ui,sans-serif",
        "serif"    => "'Lora',serif",
        "moderna"  => "'Poppins',sans-serif",
        "classica" => "'Playfair Display',serif",
        _          => null
    };

    // ── Nav / Rodapé (estruturais) ────────────────────────────────────────

    private static async Task<string> GerarNavAsync(
        string aplicacaoId, ISiteRepositorio siteRepo, string? logoSrc = null, string? slug = null)
    {
        var areas = await siteRepo.ListaAreasMenuAsync(aplicacaoId);
        var links = string.Concat(areas
            .Where(a => !string.IsNullOrEmpty(a.Nome) && !string.IsNullOrEmpty(a.Url))
            .Select(a =>
            {
                var href = slug is not null
                    ? $"/s/{WebUtility.HtmlEncode(slug)}/{WebUtility.HtmlEncode(a.Url!)}"
                    : $"/{WebUtility.HtmlEncode(a.Url!)}";
                return $"<a class=\"v-nav__link\" href=\"{href}\">{WebUtility.HtmlEncode(a.Nome!)}</a>";
            }));

        var logoHtml = !string.IsNullOrEmpty(logoSrc)
            ? $"<img class=\"v-nav__logo\" src=\"{logoSrc}\" alt=\"Logo\" />"
            : "<span class=\"v-nav__logo\" data-vitrine-logo></span>";

        return $"<nav class=\"v-nav v-nav--topo\">" +
               $"<div class=\"v-container v-nav__inner\">" +
               $"{logoHtml}" +
               $"<div class=\"v-nav__links\">{links}</div>" +
               $"</div></nav>";
    }

    private static string GerarRodape() =>
        $"<footer class=\"v-rodape v-rodape--simples\">" +
        $"<div class=\"v-container\">" +
        $"<span class=\"v-rodape__copy\">&copy; {DateTime.UtcNow.Year}</span>" +
        $"</div></footer>";

    // ── Dispatcher ────────────────────────────────────────────────────────

    private static Task<string> GerarSecaoAsync(
        SecaoConfig secao, string aplicacaoId, ISiteRepositorio siteRepo) =>
        secao switch
        {
            HeroConfig h            => Task.FromResult(GerarHero(h)),
            SobreConfig s           => Task.FromResult(GerarSobre(s)),
            CtaBannerConfig c       => Task.FromResult(GerarCtaBanner(c)),
            ListaProdutosConfig p   => GerarListaProdutosAsync(p, aplicacaoId, siteRepo),
            ListaConteudosConfig c  => GerarListaConteudosAsync(c, siteRepo),
            ListaCategoriasConfig c => GerarListaCategoriasAsync(c, aplicacaoId, siteRepo),
            DepoimentosConfig d     => Task.FromResult(GerarPlaceholder("depoimentos", d.Variante, d.Titulo)),
            ContadorConfig c        => Task.FromResult(GerarPlaceholder("contador", null, c.Titulo)),
            FaqConfig f             => Task.FromResult(GerarPlaceholder("faq", null, f.Titulo)),
            FormularioConfig f      => Task.FromResult(GerarPlaceholder("formulario", null, f.Titulo)),
            _                       => Task.FromResult("")
        };

    // ── Estáticas ─────────────────────────────────────────────────────────

    private static string GerarHero(HeroConfig h)
    {
        var subtitulo = h.Subtitulo is not null
            ? $"<p class=\"v-hero__subtitulo\">{WebUtility.HtmlEncode(h.Subtitulo)}</p>"
            : "";
        var cta = h.Cta is not null
            ? $"<a href=\"{WebUtility.HtmlEncode(h.Cta.Url)}\" class=\"v-btn v-btn--{h.Cta.Variante} v-hero__cta\">{WebUtility.HtmlEncode(h.Cta.Texto)}</a>"
            : "";
        var titulo = $"<h1 class=\"v-hero__titulo\">{WebUtility.HtmlEncode(h.Titulo)}</h1>";

        string conteudo;
        if (h.Variante == "com-imagem")
        {
            var img = !string.IsNullOrEmpty(h.ImagemUrl)
                ? $"<img class=\"v-hero__imagem\" src=\"{WebUtility.HtmlEncode(h.ImagemUrl)}\" alt=\"\" loading=\"lazy\">"
                : "";
            conteudo = $"<div class=\"v-hero__conteudo\">{titulo}{subtitulo}{cta}</div>{img}";
        }
        else
        {
            conteudo = $"{titulo}{subtitulo}{cta}";
        }

        return $"<section class=\"v-secao v-hero v-hero--{h.Variante}\">" +
               $"<div class=\"v-container\">{conteudo}</div></section>";
    }

    private static string GerarSobre(SobreConfig s)
    {
        var imagem = !string.IsNullOrEmpty(s.ImagemUrl)
            ? $"<img class=\"v-sobre__imagem\" src=\"{WebUtility.HtmlEncode(s.ImagemUrl)}\" alt=\"\" loading=\"lazy\">"
            : "";
        return $"<section class=\"v-secao v-sobre v-sobre--{s.Variante}\">" +
               $"<div class=\"v-container\">" +
               $"<div class=\"v-sobre__texto-wrapper\">" +
               $"<h2 class=\"v-sobre__titulo\">{WebUtility.HtmlEncode(s.Titulo)}</h2>" +
               $"<p class=\"v-sobre__texto\">{WebUtility.HtmlEncode(s.Texto)}</p>" +
               $"</div>{imagem}</div></section>";
    }

    private static string GerarCtaBanner(CtaBannerConfig c)
    {
        var subtitulo = c.Subtitulo is not null
            ? $"<p class=\"v-cta-banner__subtitulo\">{WebUtility.HtmlEncode(c.Subtitulo)}</p>"
            : "";
        return $"<section class=\"v-secao v-cta-banner v-cta-banner--{c.Variante}\">" +
               $"<div class=\"v-container\">" +
               $"<h2 class=\"v-cta-banner__titulo\">{WebUtility.HtmlEncode(c.Titulo)}</h2>" +
               $"{subtitulo}" +
               $"<a href=\"{WebUtility.HtmlEncode(c.Cta.Url)}\" class=\"v-btn v-btn--primario v-cta-banner__cta\">{WebUtility.HtmlEncode(c.Cta.Texto)}</a>" +
               $"</div></section>";
    }

    // ── Dinâmicas ─────────────────────────────────────────────────────────

    private static async Task<string> GerarListaProdutosAsync(
        ListaProdutosConfig p, string aplicacaoId, ISiteRepositorio siteRepo)
    {
        var produtos = await siteRepo.ListaProdutosAsync(
            aplicacaoId,
            string.IsNullOrEmpty(p.Cateriaid) ? null : p.Cateriaid,
            p.Limite ?? 8);

        const string svg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='150'%3E%3Crect width='200' height='150' fill='%23f3f4f6' rx='4'/%3E%3C/svg%3E";
        var items = string.Concat(produtos.Select(prod =>
        {
            var src = string.IsNullOrEmpty(prod.Imagem) ? svg : prod.Imagem;
            return $"<div class=\"v-produto\">" +
                   $"<img class=\"v-produto__imagem\" src=\"{src}\" alt=\"{WebUtility.HtmlEncode(prod.Nome ?? "")}\" loading=\"lazy\">" +
                   $"<h3 class=\"v-produto__nome\">{WebUtility.HtmlEncode(prod.Nome ?? "")}</h3>" +
                   $"<p class=\"v-produto__desc\">{WebUtility.HtmlEncode(prod.Descricacurta ?? "")}</p>" +
                   $"<span class=\"v-produto__preco\">R$ {prod.Valor?.ToString("F2") ?? "—"}</span>" +
                   $"</div>";
        }));

        var titulo = p.Titulo is not null
            ? $"<h2 class=\"v-secao__titulo\">{WebUtility.HtmlEncode(p.Titulo)}</h2>"
            : "";
        return $"<section class=\"v-secao v-lista-produtos v-lista-produtos--{p.Variante}\">" +
               $"<div class=\"v-container\">{titulo}<div class=\"v-lista-produtos__grid\">{items}</div></div></section>";
    }

    private static async Task<string> GerarListaConteudosAsync(
        ListaConteudosConfig c, ISiteRepositorio siteRepo)
    {
        if (string.IsNullOrEmpty(c.Areaid))
            return GerarPlaceholder("lista-conteudos", c.Variante, c.Titulo);

        var conteudos = await siteRepo.ListaConteudosPorAreaAsync(c.Areaid, c.Limite ?? 6);
        var items = string.Concat(conteudos.Select(cont =>
        {
            var texto = cont.Texto ?? "";
            var resumo = texto.Length > 120 ? texto[..120] + "…" : texto;
            return $"<article class=\"v-conteudo\">" +
                   $"<h3 class=\"v-conteudo__titulo\">{WebUtility.HtmlEncode(cont.Titulo ?? "")}</h3>" +
                   $"<p class=\"v-conteudo__resumo\">{WebUtility.HtmlEncode(resumo)}</p>" +
                   $"<time class=\"v-conteudo__data\">{cont.Datainclusao:d}</time>" +
                   $"</article>";
        }));

        var titulo = c.Titulo is not null
            ? $"<h2 class=\"v-secao__titulo\">{WebUtility.HtmlEncode(c.Titulo)}</h2>"
            : "";
        return $"<section class=\"v-secao v-lista-conteudos v-lista-conteudos--{c.Variante}\">" +
               $"<div class=\"v-container\">{titulo}{items}</div></section>";
    }

    private static async Task<string> GerarListaCategoriasAsync(
        ListaCategoriasConfig c, string aplicacaoId, ISiteRepositorio siteRepo)
    {
        var cats = await siteRepo.ListaCategoriasAsync(
            aplicacaoId,
            string.IsNullOrEmpty(c.Cateriaidpai) ? null : c.Cateriaidpai);

        var cls = c.Variante == "chips" ? "v-categoria v-categoria--chip" : "v-categoria";
        var items = string.Concat(cats.Select(cat =>
            $"<a class=\"{cls}\" href=\"/{WebUtility.HtmlEncode(cat.Cateriaid ?? "")}\">{WebUtility.HtmlEncode(cat.Nome ?? "")}</a>"));

        var titulo = c.Titulo is not null
            ? $"<h2 class=\"v-secao__titulo\">{WebUtility.HtmlEncode(c.Titulo)}</h2>"
            : "";
        return $"<section class=\"v-secao v-lista-categorias v-lista-categorias--{c.Variante}\">" +
               $"<div class=\"v-container\">{titulo}{items}</div></section>";
    }

    // ── Placeholder (depoimentos, contador, faq, formulario) ──────────────

    private static string GerarPlaceholder(string tipo, string? variante, string? titulo)
    {
        var cls = variante is not null ? $"v-{tipo} v-{tipo}--{variante}" : $"v-{tipo}";
        var label = titulo is not null ? WebUtility.HtmlEncode(titulo) : tipo;
        return $"<section class=\"v-secao {cls}\" data-pendente=\"{tipo}\">" +
               $"<div class=\"v-container\"><h2 class=\"v-secao__titulo\">{label}</h2></div></section>";
    }
}
