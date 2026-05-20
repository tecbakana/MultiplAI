using System.Text.Json;
using System.Text.Json.Serialization;

namespace ICMSX;

// ── Raiz ──────────────────────────────────────────────────────────────────

public record VitrineConfig(TemaConfig Tema, IReadOnlyList<SecaoConfig> Secoes);

// ── Tema ──────────────────────────────────────────────────────────────────

public record TemaConfig(
    string CorPrimaria,
    string? CorSecundaria,
    string? CorFundo,
    string? CorTexto,
    string? FonteTitulo,
    string? FonteCorpo,
    string Espacamento,
    string RaioBorda);

// ── Seções ────────────────────────────────────────────────────────────────

[JsonConverter(typeof(SecaoConfigConverter))]
public abstract record SecaoConfig(string Tipo);

public record CtaItemConfig(string Texto, string Url, string Variante);

// Estáticas
public record HeroConfig(
    string Variante,
    string Titulo,
    string? Subtitulo,
    string? ImagemUrl,
    CtaItemConfig? Cta) : SecaoConfig("hero");

public record SobreConfig(
    string Variante,
    string Titulo,
    string Texto,
    string? ImagemUrl) : SecaoConfig("sobre");

public record CtaBannerConfig(
    string Variante,
    string Titulo,
    string? Subtitulo,
    CtaItemConfig Cta) : SecaoConfig("cta-banner");

// Dinâmicas
public record ListaProdutosConfig(
    string Variante,
    string? Titulo,
    int? Limite,
    string? Cateriaid) : SecaoConfig("lista-produtos");

public record ListaConteudosConfig(
    string Variante,
    string? Titulo,
    string? Areaid,
    int? Limite) : SecaoConfig("lista-conteudos");

public record ListaCategoriasConfig(
    string Variante,
    string? Titulo,
    string? Cateriaidpai) : SecaoConfig("lista-categorias");

public record DepoimentosConfig(
    string Variante,
    string? Titulo,
    int? Limite) : SecaoConfig("depoimentos");

public record ContadorConfig(string? Titulo) : SecaoConfig("contador");

public record FaqConfig(string? Titulo, string? Formularioid) : SecaoConfig("faq");

public record FormularioConfig(string? Titulo, string? Formularioid) : SecaoConfig("formulario");

// ── Converter ─────────────────────────────────────────────────────────────

internal sealed class SecaoConfigConverter : JsonConverter<SecaoConfig>
{
    public override SecaoConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var tipo = doc.RootElement.GetProperty("tipo").GetString()
            ?? throw new JsonException("Propriedade 'tipo' ausente na seção.");
        var raw = doc.RootElement.GetRawText();

        return tipo switch
        {
            "hero"             => JsonSerializer.Deserialize<HeroConfig>(raw, options),
            "sobre"            => JsonSerializer.Deserialize<SobreConfig>(raw, options),
            "cta-banner"       => JsonSerializer.Deserialize<CtaBannerConfig>(raw, options),
            "lista-produtos"   => JsonSerializer.Deserialize<ListaProdutosConfig>(raw, options),
            "lista-conteudos"  => JsonSerializer.Deserialize<ListaConteudosConfig>(raw, options),
            "lista-categorias" => JsonSerializer.Deserialize<ListaCategoriasConfig>(raw, options),
            "depoimentos"      => JsonSerializer.Deserialize<DepoimentosConfig>(raw, options),
            "contador"         => JsonSerializer.Deserialize<ContadorConfig>(raw, options),
            "faq"              => JsonSerializer.Deserialize<FaqConfig>(raw, options),
            "formulario"       => JsonSerializer.Deserialize<FormularioConfig>(raw, options),
            _ => throw new JsonException($"Tipo de seção desconhecido: '{tipo}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, SecaoConfig value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, (object)value, options);
}

// ── Opções padrão para desserializar JSON da IA ───────────────────────────

public static class VitrineJsonOptions
{
    public static readonly JsonSerializerOptions Padrao = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
