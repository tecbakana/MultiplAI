using FluentAssertions;

namespace CMSX.Tests.Architecture;

/// <summary>
/// Varredura de código-fonte para padrões proibidos pelo CLAUDE.md.
/// Complementa os testes de reflexão cobrindo o que o IL não expõe em tempo de compilação.
/// </summary>
public class SourceCodeGuardTests
{
    private static readonly string RootDir = ResolveRootDir();

    private static string ResolveRootDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "CMSX.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Raiz do CMSX não encontrada.");
    }

    private static IEnumerable<(string File, int Line, string Content)> ScanarArquivos(
        string[] pastas, string padrao, string[] excluir)
    {
        foreach (var pasta in pastas)
        {
            var dir = Path.Combine(RootDir, pasta);
            if (!Directory.Exists(dir)) continue;

            foreach (var arquivo in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            {
                if (excluir.Any(e => arquivo.Contains(e))) continue;

                var linhas = File.ReadAllLines(arquivo);
                for (int i = 0; i < linhas.Length; i++)
                {
                    var linha = linhas[i];
                    if (linha.TrimStart().StartsWith("//")) continue; // ignora comentários
                    if (linha.Contains(padrao))
                        yield return (Path.GetRelativePath(RootDir, arquivo), i + 1, linha.Trim());
                }
            }
        }
    }

    [Fact]
    public void Controllers_NaoDevem_Usar_DateTimeNow()
    {
        var pastas  = new[] { "CMSAPI", "CMSAPI.Publica" };
        var excluir = new[] { "obj", "bin" };

        var violacoes = ScanarArquivos(pastas, "DateTime.Now", excluir).ToList();

        violacoes.Should().BeEmpty(
            because: "use DateTime.UtcNow — DateTime.Now causa falhas silenciosas no PostgreSQL e inconsistências de fuso");
    }

    [Fact]
    public void Repositorios_NaoDevem_Usar_DateTimeNow()
    {
        var pastas  = new[] { "CMSXRepo" };
        var excluir = new[] { "obj", "bin" };

        var violacoes = ScanarArquivos(pastas, "DateTime.Now", excluir).ToList();

        violacoes.Should().BeEmpty(
            because: "use DateTime.UtcNow — DateTime.Now com Kind=Local falha explicitamente no PostgreSQL");
    }

    [Fact]
    public void Repositorios_NaoDevem_Usar_ToString_EmLinq()
    {
        // Detecta .ToString() dentro de expressões Where/Select/FirstOrDefault
        // que é o padrão proibido — conversões dentro de IQueryable violam SARGability
        var pastas  = new[] { "CMSXRepo" };
        var excluir = new[] { "obj", "bin" };

        var linqMetodos = new[] { ".Where(", ".Select(", ".FirstOrDefault(", ".Any(", ".OrderBy(" };

        var arquivosComLinqEToString = new List<string>();

        foreach (var pasta in pastas)
        {
            var dir = Path.Combine(RootDir, pasta);
            if (!Directory.Exists(dir)) continue;

            foreach (var arquivo in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            {
                if (excluir.Any(e => arquivo.Contains(e))) continue;

                var conteudo = File.ReadAllText(arquivo);
                var temLinq  = linqMetodos.Any(m => conteudo.Contains(m));
                if (!temLinq) continue;

                // Verifica se .ToString() aparece dentro de lambdas LINQ no mesmo bloco
                var linhas = conteudo.Split('\n');
                for (int i = 0; i < linhas.Length; i++)
                {
                    var linha = linhas[i];
                    if (linha.TrimStart().StartsWith("//")) continue;
                    if (linqMetodos.Any(m => linha.Contains(m)) && linha.Contains(".ToString()"))
                        arquivosComLinqEToString.Add($"{Path.GetRelativePath(RootDir, arquivo)}:{i + 1} — {linha.Trim()}");
                }
            }
        }

        arquivosComLinqEToString.Should().BeEmpty(
            because: "conversões dentro de IQueryable quebram no PostgreSQL e violam SARGability — converta antes de entrar no repositório");
    }

    [Fact]
    public void Controllers_NaoDevem_Referenciar_CMSXRepo_Diretamente()
    {
        var pastas  = new[] { "CMSAPI", "CMSAPI.Publica" };
        var excluir = new[] { "obj", "bin" };

        // Detecta uso direto de namespace CMSXRepo em controllers
        // Program.cs precisa referenciar CMSXRepo para registro de DI — é o único ponto permitido
        var excluirComProgram = excluir.Append("Program.cs").ToArray();
        var violacoes = ScanarArquivos(pastas, "using CMSXRepo", excluirComProgram).ToList();

        violacoes.Should().BeEmpty(
            because: "controllers não devem referenciar CMSXRepo diretamente — injetar somente interfaces de ICMSX");
    }

    [Fact]
    public void NenhumArquivo_DeveInstanciar_CmsxDbContext_Diretamente()
    {
        var pastas  = new[] { "CMSAPI", "CMSAPI.Publica", "ICMSX" };
        var excluir = new[] { "obj", "bin" };

        var violacoes = ScanarArquivos(pastas, "new CmsxDbContext", excluir).ToList();

        violacoes.Should().BeEmpty(
            because: "CmsxDbContext é instanciado pelo DI — nunca instanciar com 'new' fora de testes");
    }
}
