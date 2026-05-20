using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using CMSAPI.Services;
using ICMSX;

namespace CMSAPI.Controllers;

[ApiController]
[Route("Admin/vitrine")]
[Authorize]
public class VitrineAdminController : Controller
{
    private readonly IVitrineRepositorio _repo;
    private readonly IAgentIAFactory _agentFactory;

    public VitrineAdminController(IVitrineRepositorio repo, IAgentIAFactory agentFactory)
    {
        _repo = repo;
        _agentFactory = agentFactory;
    }

    private bool IsAdmin() => User.FindFirstValue("acessoTotal") == "True";

    [HttpPost("templates/gerar")]
    public async Task<IActionResult> GerarTemplate([FromBody] VitrineGerarInput input)
    {
        if (!IsAdmin()) return Forbid();

        var promptBase = $"""
            Gere uma landing page HTML completa e visualmente impactante para: {input.Prompt}

            Regras obrigatórias:
            - HTML semântico completo com <style> interno
            - Use CSS custom properties para todas as cores, fontes e espaçamentos principais: --cor-primaria, --cor-secundaria, --fonte-titulo, --fonte-corpo
            - Textos editáveis marcados com data-vitrine-texto="chave"
            - Imagens editáveis marcadas com data-vitrine-imagem="chave" (o src padrão deve ser um SVG data URI — veja abaixo)
            - Layout responsivo (mobile-first)
            - SVG inline para elementos decorativos
            - Sem dependências externas exceto Google Fonts
            - PROIBIDO usar URLs externas de imagem (via.placeholder.com, picsum.photos, placehold.co, lorempixel, unsplash, etc.)
            - Para placeholder de imagem use exclusivamente SVG data URI. Exemplo para logo 120x40:
              <img data-vitrine-imagem="logo" src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='120' height='40'%3E%3Crect width='120' height='40' fill='%23ccc' rx='4'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%23555' font-size='13' font-family='sans-serif'%3ELogo%3C/text%3E%3C/svg%3E">
            - Retornar apenas HTML válido, sem markdown
            - OBRIGATÓRIO: O nav principal DEVE ter data-vitrine-slot="nav". Os links <a> internos serão injetados dinamicamente; não crie links hardcoded dentro desse nav. Estilize os filhos via seletor [data-vitrine-slot="nav"] a no CSS interno. Exemplo:
              <nav data-vitrine-slot="nav" style="display:flex; gap:1.5rem;"><!-- links injetados --></nav>
            - OBRIGATÓRIO: O template HTML DEVE conter exatamente este elemento onde o conteúdo da página será inserido:
              <div data-vitrine-slot="conteudo" style="min-height: 200px;"><!-- conteúdo da página --></div>
              Posicione-o entre o header e o footer da vitrine.
            - Para seções de dados dinâmicos (produtos, artigos, categorias) NUNCA escreva itens hardcoded — use marcadores de bloco:
              <section data-vitrine-bloco="lista-produtos" data-limite="8" data-cateriaid=""></section>
              <section data-vitrine-bloco="lista-conteudos" data-limite="6" data-areaid=""></section>
              <section data-vitrine-bloco="lista-categorias" data-cateriaidpai=""></section>
            - O CSS interno DEVE estilizar obrigatoriamente as classes fixas do sistema para cada bloco:
              .vitrine-produto, .vitrine-produto-img, .vitrine-produto-nome, .vitrine-produto-desc, .vitrine-produto-preco
              .vitrine-conteudo, .vitrine-conteudo-titulo, .vitrine-conteudo-resumo, .vitrine-conteudo-data
              .vitrine-categoria

            REGRA OBRIGATÓRIA — SLOTS:
            Todo HTML gerado DEVE conter zonas editáveis declaradas com o atributo data-vitrine-slot="nome".
            Exemplo de declaração: <section class="hero"><div data-vitrine-slot="hero-titulo"></div></section>
            Regras:
            1. Mínimo de 3 slots por layout
            2. Nomes em kebab-case descritivos: hero-titulo, hero-subtitulo, hero-cta, sobre-texto, produtos-grid, rodape-contato
            3. O elemento com data-vitrine-slot deve ter conteúdo padrão HTML (placeholder) dentro — será substituído em runtime
            4. Slots são obrigatórios mesmo que o layout seja simples
            Sem slots declarados o HTML será rejeitado.
            """;

        var agente = _agentFactory.Criar(provedor: input.Provedor);
        string html;

        if (!string.IsNullOrWhiteSpace(input.ImagemBase64))
        {
            var bytes = Convert.FromBase64String(input.ImagemBase64);
            html = await agente.GerarComImagemAsync(bytes, "image/jpeg", promptBase);
        }
        else
        {
            html = await agente.GerarAsync(promptBase);
        }

        var variaveisJson = ExtrairVariaveis(html);
        return Ok(new { htmlCss = html, variaveisJson });
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CriarTemplate([FromBody] VitrineTemplateInput input)
    {
        if (!IsAdmin()) return Forbid();
        var id = await _repo.CriarTemplateAsync(input);
        return Ok(new { vitrineTemplateId = id });
    }

    [HttpGet("templates")]
    public async Task<IActionResult> ListaTemplates()
    {
        if (!IsAdmin()) return Forbid();
        return Ok(await _repo.ListaTemplatesAsync());
    }

    [HttpGet("templates/{id:guid}")]
    public async Task<IActionResult> BuscaTemplate(Guid id)
    {
        if (!IsAdmin()) return Forbid();
        var template = await _repo.BuscaTemplateAsync(id);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPut("templates/{id:guid}")]
    public async Task<IActionResult> AtualizarTemplate(Guid id, [FromBody] VitrineTemplateInput input)
    {
        if (!IsAdmin()) return Forbid();
        var ok = await _repo.AtualizarTemplateAsync(id, input);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("templates/{id:guid}")]
    public async Task<IActionResult> DesativarTemplate(Guid id)
    {
        if (!IsAdmin()) return Forbid();
        var ok = await _repo.DesativarTemplateAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static string ExtrairVariaveis(string html)
    {
        var variaveis = new List<object>();
        var vistasCSS = new HashSet<string>();
        var vistasTexto = new HashSet<string>();
        var vistasImg = new HashSet<string>();

        foreach (Match m in Regex.Matches(html, @"(--[\w-]+)\s*:\s*([^;}\n]+)"))
        {
            var chave = m.Groups[1].Value.Trim();
            var padrao = m.Groups[2].Value.Trim();
            if (!vistasCSS.Add(chave)) continue;
            var tipo = chave.Contains("cor") || chave.Contains("color") ? "color"
                     : chave.Contains("fonte") || chave.Contains("font") ? "font"
                     : "text";
            variaveis.Add(new { chave, label = chave.TrimStart('-').Replace("-", " "), tipo, padrao });
        }

        foreach (Match m in Regex.Matches(html, @"data-vitrine-texto=""([\w-]+)"""))
        {
            var chave = m.Groups[1].Value;
            if (vistasTexto.Add(chave))
                variaveis.Add(new { chave, label = chave.Replace("-", " "), tipo = "text", padrao = (string?)null });
        }

        foreach (Match m in Regex.Matches(html, @"data-vitrine-imagem=""([\w-]+)"""))
        {
            var chave = m.Groups[1].Value;
            if (vistasImg.Add(chave))
                variaveis.Add(new { chave, label = chave.Replace("-", " "), tipo = "image", padrao = (string?)null });
        }

        // Pass 4: data-vitrine-bloco — atributos de configuração de blocos dinâmicos
        var vistasBlocos = new HashSet<string>();
        foreach (Match mBloco in Regex.Matches(html, @"<[^>]*data-vitrine-bloco=""([\w-]+)""([^>]*)>", RegexOptions.IgnoreCase))
        {
            var tipoBloco = mBloco.Groups[1].Value;
            var attrs = mBloco.Groups[2].Value;
            ExtBloco("limite", "number");
            ExtBloco("cateriaid", "text");
            ExtBloco("areaid", "text");
            ExtBloco("cateriaidpai", "text");

            void ExtBloco(string attrName, string tipo)
            {
                var mAttr = Regex.Match(attrs, $@"data-{attrName}=""([^""]*)""", RegexOptions.IgnoreCase);
                if (!mAttr.Success) return;
                var chave = $"{tipoBloco}__{attrName}";
                if (!vistasBlocos.Add(chave)) return;
                var padrao = string.IsNullOrEmpty(mAttr.Groups[1].Value) ? null : mAttr.Groups[1].Value;
                variaveis.Add(new { chave, label = $"{tipoBloco}: {attrName}", tipo, padrao });
            }
        }

        return JsonSerializer.Serialize(variaveis);
    }
}
