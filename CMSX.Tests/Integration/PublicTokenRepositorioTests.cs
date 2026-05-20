using CMSXData.Models;
using CMSXRepo;
using FluentAssertions;

namespace CMSX.Tests.Integration;

// ── SQL Server ────────────────────────────────────────────────────────────────

[Collection("SqlServer")]
public class PublicTokenRepositorioSqlServerTests(SqlServerFixture db)
    : PublicTokenRepositorioTestsBase(() => db.CreateContext());

// ── PostgreSQL ────────────────────────────────────────────────────────────────

[Collection("PostgreSql")]
public class PublicTokenRepositorioPostgreSqlTests(PostgreSqlFixture db)
    : PublicTokenRepositorioTestsBase(() => db.CreateContext());

// ── Casos de teste compartilhados ─────────────────────────────────────────────

public abstract class PublicTokenRepositorioTestsBase
{
    private readonly Func<CmsxDbContext> _ctxFactory;

    protected PublicTokenRepositorioTestsBase(Func<CmsxDbContext> ctxFactory)
    {
        _ctxFactory = ctxFactory;
    }

    private static PublicTokenRepositorio Repo(CmsxDbContext ctx) => new(ctx);

    private static PublicToken NovoToken(string aplicacaoid, DateTime? vencimento = null) => new()
    {
        PublicTokenId  = Guid.NewGuid(),
        Token          = Guid.NewGuid().ToString("N"),
        Aplicacaoid    = aplicacaoid,
        Ativo          = true,
        Datainclusao   = DateTime.UtcNow,
        Datavencimento = vencimento
    };

    [Fact]
    public async Task Lista_RetornaVazia_QuandoAplicacaoNaoExiste()
    {
        using var ctx = _ctxFactory();
        (await Repo(ctx).ListaAsync(Guid.NewGuid().ToString("N")))
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Criar_E_Lista_RetornaToken()
    {
        var appId = Guid.NewGuid().ToString("N");

        using var ctx = _ctxFactory();
        await Repo(ctx).CriarAsync(NovoToken(appId));

        using var ctx2 = _ctxFactory();
        (await Repo(ctx2).ListaAsync(appId))
            .Should().ContainSingle(t => t.Aplicacaoid == appId);
    }

    [Fact]
    public async Task BuscaPorId_RetornaToken_QuandoExiste()
    {
        var appId = Guid.NewGuid().ToString("N");
        var token = NovoToken(appId);

        using var ctx = _ctxFactory();
        await Repo(ctx).CriarAsync(token);

        using var ctx2 = _ctxFactory();
        var encontrado = await Repo(ctx2).BuscaPorIdAsync(token.PublicTokenId);
        encontrado.Should().NotBeNull();
        encontrado!.Aplicacaoid.Should().Be(appId);
    }

    [Fact]
    public async Task BuscaPorId_RetornaNull_QuandoNaoExiste()
    {
        using var ctx = _ctxFactory();
        (await Repo(ctx).BuscaPorIdAsync(Guid.NewGuid()))
            .Should().BeNull();
    }

    [Fact]
    public async Task Revogar_MarcaComoInativo()
    {
        var appId = Guid.NewGuid().ToString("N");
        var token = NovoToken(appId);

        using var ctx = _ctxFactory();
        await Repo(ctx).CriarAsync(token);

        using var ctx2 = _ctxFactory();
        var repo2 = Repo(ctx2);
        var encontrado = await repo2.BuscaPorIdAsync(token.PublicTokenId);
        encontrado.Should().NotBeNull();
        await repo2.RevogarAsync(encontrado!);

        using var ctx3 = _ctxFactory();
        var resultado = await Repo(ctx3).BuscaPorIdAsync(token.PublicTokenId);
        resultado!.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Lista_NaoRetorna_TokensDeOutroTenant()
    {
        var appId1 = Guid.NewGuid().ToString("N");
        var appId2 = Guid.NewGuid().ToString("N");

        using var ctx = _ctxFactory();
        await Repo(ctx).CriarAsync(NovoToken(appId1));
        await Repo(ctx).CriarAsync(NovoToken(appId2));

        using var ctx2 = _ctxFactory();
        var lista = await Repo(ctx2).ListaAsync(appId1);
        lista.Should().OnlyContain(t => t.Aplicacaoid == appId1);
    }

    [Fact]
    public async Task Datavencimento_UtcNao_QueuebraNoPostgres()
    {
        // Valida que DateTime com Kind=Utc é persistido e recuperado corretamente em ambos os bancos.
        var appId = Guid.NewGuid().ToString("N");
        var vencimento = DateTime.SpecifyKind(new DateTime(2030, 1, 1, 0, 0, 0), DateTimeKind.Utc);
        var token = NovoToken(appId, vencimento);

        using var ctx = _ctxFactory();
        await Repo(ctx).CriarAsync(token);

        using var ctx2 = _ctxFactory();
        var encontrado = await Repo(ctx2).BuscaPorIdAsync(token.PublicTokenId);
        encontrado!.Datavencimento.Should().NotBeNull();
        encontrado.Datavencimento!.Value.Should().BeCloseTo(vencimento, TimeSpan.FromSeconds(1));
    }
}
