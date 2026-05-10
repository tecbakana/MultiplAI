namespace ICMSX;

public record OrcamentoItemInput(string Descricao, decimal Quantidade, decimal? Valor);

public record OrcamentoInput(
    string Aplicacaoid,
    string Nome,
    string? Email,
    string? Telefone,
    string? Descricaoservico,
    decimal? Valorestimado,
    string? Prazo,
    string? Nomevendedor,
    IEnumerable<OrcamentoItemInput> Itens);
