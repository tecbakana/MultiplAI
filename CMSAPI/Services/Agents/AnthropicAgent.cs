using System.Net.Http.Json;
using System.Text.Json;
using ICMSX;

namespace CMSAPI.Services.Agents;

public class AnthropicAgent : IAgentIA
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _url;
    private readonly IHttpClientFactory _httpFactory;

    public AnthropicAgent(string apiKey, string model, string url, IHttpClientFactory httpFactory)
    {
        _apiKey = apiKey;
        _model = model;
        _url = url;
        _httpFactory = httpFactory;
    }

    public string Provedor => "anthropic";

    public async Task<string> GerarAsync(string prompt)
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model = _model,
            max_tokens = 2048,
            messages = new[] { new { role = "user", content = prompt } }
        };

        var response = await http.PostAsJsonAsync(_url, body);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString()!;
    }

    public async Task<string> GerarComSistemaAsync(string systemPrompt, string userPrompt)
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model = _model,
            max_tokens = 4096,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = userPrompt } }
        };

        var response = await http.PostAsJsonAsync(_url, body);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString()!;
    }

    public async Task<string> GerarComImagemAsync(byte[] imageBytes, string mimeType, string prompt)
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model = _model,
            max_tokens = 1024,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "image", source = new { type = "base64", media_type = mimeType, data = Convert.ToBase64String(imageBytes) } },
                        new { type = "text", text = prompt }
                    }
                }
            }
        };

        var response = await http.PostAsJsonAsync(_url, body);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString()!;
    }
}