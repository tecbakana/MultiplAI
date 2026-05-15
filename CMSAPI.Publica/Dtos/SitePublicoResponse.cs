namespace CMSAPIPublica.Dtos;

public record SitePublicoResponse(
    string? Nome,
    string? Url,
    string? Header,
    IEnumerable<AreaPublicoResponse> Areas
);
