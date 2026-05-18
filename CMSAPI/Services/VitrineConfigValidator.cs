using System.Text.RegularExpressions;
using ICMSX;

namespace CMSAPI.Services;

public static class VitrineConfigValidator
{
    private static readonly HashSet<string> EspacamentosValidos = ["compacto", "confortavel", "generoso"];
    private static readonly HashSet<string> RaiosBordaValidos   = ["nenhum", "suave", "arredondado"];
    private static readonly HashSet<string> FontesValidas       = ["sans", "serif", "moderna", "classica"];
    private static readonly Regex HexRegex = new(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    private static readonly Dictionary<string, HashSet<string>> VariantesValidas = new()
    {
        ["hero"]             = ["centralizado", "esquerda", "com-imagem"],
        ["sobre"]            = ["texto-esquerda", "texto-direita", "centralizado"],
        ["cta-banner"]       = ["simples", "gradiente"],
        ["lista-produtos"]   = ["grade", "carrossel", "destaque"],
        ["lista-conteudos"]  = ["grade", "lista"],
        ["lista-categorias"] = ["grade", "chips"],
        ["depoimentos"]      = ["carrossel", "grade"],
    };

    public static List<string> Validar(VitrineConfig config)
    {
        var erros = new List<string>();

        ValidarTema(config.Tema, erros);

        if (config.Secoes is null || config.Secoes.Count == 0)
        {
            erros.Add("secoes: obrigatório e não pode ser vazio.");
            return erros;
        }

        if (config.Secoes.Count > 8)
            erros.Add($"secoes: máximo 8 seções, encontrado {config.Secoes.Count}.");

        if (config.Secoes[0].Tipo != "hero")
            erros.Add("secoes[0]: a primeira seção deve ser do tipo 'hero'.");

        for (var i = 0; i < config.Secoes.Count; i++)
            ValidarSecao(config.Secoes[i], i, erros);

        return erros;
    }

    private static void ValidarTema(TemaConfig tema, List<string> erros)
    {
        if (string.IsNullOrWhiteSpace(tema.CorPrimaria) || !HexRegex.IsMatch(tema.CorPrimaria))
            erros.Add("tema.corPrimaria: obrigatório no formato #HEX (ex: #E67E22).");

        foreach (var (campo, valor) in new[]
        {
            ("tema.corSecundaria", tema.CorSecundaria),
            ("tema.corFundo",      tema.CorFundo),
            ("tema.corTexto",      tema.CorTexto),
        })
        {
            if (valor is not null && !HexRegex.IsMatch(valor))
                erros.Add($"{campo}: quando presente, deve estar no formato #HEX.");
        }

        if (tema.FonteTitulo is not null && !FontesValidas.Contains(tema.FonteTitulo))
            erros.Add($"tema.fonteTitulo: valor inválido '{tema.FonteTitulo}'. Permitido: {string.Join(", ", FontesValidas)}.");

        if (tema.FonteCorpo is not null && !FontesValidas.Contains(tema.FonteCorpo))
            erros.Add($"tema.fonteCorpo: valor inválido '{tema.FonteCorpo}'. Permitido: {string.Join(", ", FontesValidas)}.");

        if (!EspacamentosValidos.Contains(tema.Espacamento))
            erros.Add($"tema.espacamento: valor inválido '{tema.Espacamento}'. Permitido: {string.Join(", ", EspacamentosValidos)}.");

        if (!RaiosBordaValidos.Contains(tema.RaioBorda))
            erros.Add($"tema.raioBorda: valor inválido '{tema.RaioBorda}'. Permitido: {string.Join(", ", RaiosBordaValidos)}.");
    }

    private static void ValidarSecao(SecaoConfig secao, int index, List<string> erros)
    {
        var prefixo = $"secoes[{index}]";

        if (VariantesValidas.TryGetValue(secao.Tipo, out var variantes))
        {
            var variante = secao switch
            {
                HeroConfig h           => h.Variante,
                SobreConfig s          => s.Variante,
                CtaBannerConfig c      => c.Variante,
                ListaProdutosConfig p  => p.Variante,
                ListaConteudosConfig c => c.Variante,
                ListaCategoriasConfig c => c.Variante,
                DepoimentosConfig d    => d.Variante,
                _                      => null
            };

            if (variante is not null && !variantes.Contains(variante))
                erros.Add($"{prefixo}.variante: valor inválido '{variante}' para tipo '{secao.Tipo}'. Permitido: {string.Join(", ", variantes)}.");
        }

        switch (secao)
        {
            case HeroConfig h when string.IsNullOrWhiteSpace(h.Titulo):
                erros.Add($"{prefixo}: hero.titulo é obrigatório.");
                break;

            case SobreConfig s when string.IsNullOrWhiteSpace(s.Titulo) || string.IsNullOrWhiteSpace(s.Texto):
                erros.Add($"{prefixo}: sobre.titulo e sobre.texto são obrigatórios.");
                break;

            case CtaBannerConfig c when string.IsNullOrWhiteSpace(c.Titulo) || c.Cta is null:
                erros.Add($"{prefixo}: cta-banner.titulo e cta-banner.cta são obrigatórios.");
                break;

            case ListaProdutosConfig p when p.Limite is < 1 or > 50:
                erros.Add($"{prefixo}: lista-produtos.limite deve estar entre 1 e 50.");
                break;

            case ListaConteudosConfig c when c.Limite is < 1 or > 50:
                erros.Add($"{prefixo}: lista-conteudos.limite deve estar entre 1 e 50.");
                break;

            case DepoimentosConfig d when d.Limite is < 1 or > 20:
                erros.Add($"{prefixo}: depoimentos.limite deve estar entre 1 e 20.");
                break;
        }
    }
}
