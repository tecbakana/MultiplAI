using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace CMSAPI.Services;

public class MarketHubHttpService(
    HttpClient http,
    IConfiguration config,
    IMemoryCache cache,
    ILogger<MarketHubHttpService> logger) : IMarketHubHttpService
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
    private static readonly TimeSpan _tokenTtl = TimeSpan.FromHours(1);

    private async Task<string?> GetTokenAsync(string tenantId)
    {
        var cacheKey = $"mh_token_{tenantId}";
        if (cache.TryGetValue(cacheKey, out string? token))
            return token;

        var secret = config["MarketHub:Secret"]!;
        var payload = new { tenantId, secret };
        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");

        var response = await http.PostAsync("auth/token", content);
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body, _json);
        token = result.GetProperty("token").GetString();

        if (token != null)
            cache.Set(cacheKey, token, _tokenTtl);

        return token;
    }

    private async Task<HttpResponseMessage> SendAsync(string tenantId, HttpRequestMessage request)
    {
        var token = await GetTokenAsync(tenantId);
        if (token == null)
            throw new InvalidOperationException($"Tenant '{tenantId}' não provisionado no MarketHub");

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await http.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            cache.Remove($"mh_token_{tenantId}");
            var newToken = await GetTokenAsync(tenantId);
            if (newToken == null)
                throw new InvalidOperationException($"Tenant '{tenantId}' não provisionado no MarketHub");

            var retryRequest = new HttpRequestMessage(request.Method, request.RequestUri);
            if (request.Content != null)
            {
                var body = await request.Content.ReadAsStringAsync();
                retryRequest.Content = new StringContent(body,
                    System.Text.Encoding.UTF8,
                    request.Content.Headers.ContentType?.MediaType ?? "application/json");
            }
            retryRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
            response = await http.SendAsync(retryRequest);
        }

        return response;
    }

    public async Task<string?> GetConfiguracoes(string tenantId)
    {
        try
        {
            var res = await SendAsync(tenantId, new HttpRequestMessage(HttpMethod.Get, "configuracoes"));
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar configurações MarketHub — tenant {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<bool> PostConfiguracao(string tenantId, string jsonBody)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "configuracoes")
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            var res = await SendAsync(tenantId, req);
            res.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar configuração MarketHub — tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<string?> GetPedidos(string tenantId)
    {
        try
        {
            var res = await SendAsync(tenantId, new HttpRequestMessage(HttpMethod.Get, "pedidos"));
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar pedidos MarketHub — tenant {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<(bool encontrado, string? json)> GetPedido(string tenantId, string id)
    {
        try
        {
            var res = await SendAsync(tenantId, new HttpRequestMessage(HttpMethod.Get, $"pedidos/{id}"));
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (false, null);
            res.EnsureSuccessStatusCode();
            return (true, await res.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar pedido {Id} MarketHub — tenant {TenantId}", id, tenantId);
            return (false, null);
        }
    }

    public async Task<bool> DeleteConfiguracao(string tenantId, string marketplace)
    {
        try
        {
            var res = await SendAsync(tenantId, new HttpRequestMessage(HttpMethod.Delete, $"configuracoes/{marketplace}"));
            res.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao remover configuração MarketHub — tenant {TenantId}, marketplace {Marketplace}", tenantId, marketplace);
            return false;
        }
    }

    public async Task<string?> EmitirNf(string tenantId, string id)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"pedidos/{id}/emitir-nf")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            var res = await SendAsync(tenantId, req);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao emitir NF pedido {Id} MarketHub — tenant {TenantId}", id, tenantId);
            return null;
        }
    }
}
