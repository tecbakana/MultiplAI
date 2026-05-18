namespace ICMSX;

public interface IAgentIA
{
    string Provedor { get; }
    Task<string> GerarAsync(string prompt);
    Task<string> GerarComImagemAsync(byte[] imageBytes, string mimeType, string prompt);
    Task<string> GerarComSistemaAsync(string systemPrompt, string userPrompt);
}
