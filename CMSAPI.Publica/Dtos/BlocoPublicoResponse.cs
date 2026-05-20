namespace CMSAPIPublica.Dtos;

public record BlocoPublicoResponse(
    string Tipo,
    object? Config,
    string? Coluna,
    object? Dados
);
