using CMSXData.Models;
using System.Text;
using System.Text.Json;

namespace CMSAPI.Services;

public class SalematicHttpService(HttpClient http, ILogger<SalematicHttpService> logger) : ISalematicHttpService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<SalematicAuthResponse?> RegistrarAsync(RegistrarLojaRequest req)
    {
        try
        {
            var payload = new
            {
                AplicacaoId = req.Aplicacaoid,
                req.Nome, req.Documento, req.Email, req.Senha, req.Telefone,
                req.Cep, req.Logradouro, req.Numero, req.Complemento,
                req.Bairro, req.Cidade, req.Estado
            };
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await http.PostAsync("api/auth/register", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SalematicAuthResponse>(body, _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao registrar cliente na Salematic");
            return null;
        }
    }

    public async Task<SalematicAuthResponse?> LoginAsync(LoginLojaRequest req)
    {
        try
        {
            var payload = new { req.Email, req.Senha };
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await http.PostAsync("api/auth/login", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SalematicAuthResponse>(body, _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao fazer login na Salematic");
            return null;
        }
    }

    public async Task<List<ClienteLoja>> ListarClientesAsync()
    {
        try
        {
            var response = await http.GetAsync("api/clientes?id=2");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ClienteLoja>>(content, _jsonOptions) ?? new List<ClienteLoja>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao listar clientes da Salematic");
            return new List<ClienteLoja>();
        }
    }

    public async Task<ClienteLoja?> CadastrarClienteAsync(ClienteLoja cliente)
    {
        try
        {
            var json = JsonSerializer.Serialize(cliente, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await http.PostAsync("api/clientes", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ClienteLoja>(body, _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao cadastrar cliente na Salematic");
            return null;
        }
    }
}
