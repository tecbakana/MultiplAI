using System.Net.Http.Json;
using System.Text.Json;
using ICMSX;

namespace CMSAPI.Services.Agents;

public class GeminiAgent : IAgentIA
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;
    private readonly IHttpClientFactory _httpFactory;

    public GeminiAgent(string apiKey, string model, string baseUrl, IHttpClientFactory httpFactory)
    {
        _apiKey = apiKey;
        _model = model;
        _baseUrl = baseUrl;
        _httpFactory = httpFactory;
    }

    public string Provedor => "gemini";

    public async Task<string> GerarAsync(string prompt)
    {
        var http = _httpFactory.CreateClient();
        var url = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new { temperature = 0.2, maxOutputTokens = 32768 }
        };

        var response = await http.PostAsJsonAsync(url, body);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"{(int)response.StatusCode} ({response.ReasonPhrase}) — {responseText}");

        var result = JsonDocument.Parse(responseText).RootElement;
        return result
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()!;
    }

    public async Task<string> GerarComSistemaAsync(string systemPrompt, string userPrompt)
    {
        var http = _httpFactory.CreateClient();
        var url = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

        var body = new
        {
            systemInstruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = new[] { new { parts = new[] { new { text = userPrompt } } } },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 4096,
                responseMimeType = "application/json"
            }
        };

        var response = await http.PostAsJsonAsync(url, body);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"{(int)response.StatusCode} ({response.ReasonPhrase}) — {responseText}");

        var result = JsonDocument.Parse(responseText).RootElement;
        return result
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()!;
    }

    public async Task<string> GerarComImagemAsync(byte[] imageBytes, string mimeType, string prompt)
    {
        var http = _httpFactory.CreateClient();
        var url = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { inlineData = new { mimeType, data = Convert.ToBase64String(imageBytes) } },
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new { temperature = 0.2, maxOutputTokens = 4096 }
        };

        var response = await http.PostAsJsonAsync(url, body);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"{(int)response.StatusCode} ({response.ReasonPhrase}) — {responseText}");

        var result = JsonDocument.Parse(responseText).RootElement;
        return result
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()!;
    }
}