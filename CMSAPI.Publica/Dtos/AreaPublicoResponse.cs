namespace CMSAPIPublica.Dtos;

public record AreaPublicoResponse(
    string? AreaId,
    string? Nome,
    string? Url,
    bool TemLayout,
    IEnumerable<BlocoPublicoResponse> Blocos,
    string? HtmlSnapshot
);
