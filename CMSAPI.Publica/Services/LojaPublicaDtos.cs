namespace CMSAPIPublica.Services;

public class RegistrarLojaRequest
{
    public string Token { get; set; } = string.Empty;
    public string Aplicacaoid { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class LoginLojaRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class SalematicAuthResponse
{
    public string Token { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CriarPedidoLojaRequest
{
    public string Token { get; set; } = string.Empty;
    public string Numeropedido { get; set; } = string.Empty;
    public string Clientenome { get; set; } = string.Empty;
    public string Clienteemail { get; set; } = string.Empty;
    public decimal Valorpedido { get; set; }
    public string MetodoPagamento { get; set; } = string.Empty;
    public List<ItemPedidoLoja> Itens { get; set; } = new();
}

public class ItemPedidoLoja
{
    public string Produtoid { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}

public class EsqueciSenhaRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetSenhaRequest
{
    public string Token { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
