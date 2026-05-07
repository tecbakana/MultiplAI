using CMSXData.Models;
using CMSXRepo;
using FluentAssertions;

namespace CMSX.Tests.Integration;

// ── SQL Server ────────────────────────────────────────────────────────────────

[Collection("SqlServer")]
public class AplicacaoRepositorioSqlServerTests(SqlServerFixture db)
    : AplicacaoRepositorioTestsBase(() => db.CreateContext());

// ── PostgreSQL ────────────────────────────────────────────────────────────────

[Collection("PostgreSql")]
public class AplicacaoRepositorioPostgreSqlTests(PostgreSqlFixture db)
    : AplicacaoRepositorioTestsBase(() => db.CreateContext());

// ── Casos de teste compartilhados ─────────────────────────────────────────────

public abstract class AplicacaoRepositorioTestsBase
{
    private readonly Func<CmsxDbContext> _ctxFactory;

    protected AplicacaoRepositorioTestsBase(Func<CmsxDbContext> ctxFactory)
    {
        _ctxFactory = ctxFactory;
    }

    private AplicacaoRepositorio Repo(CmsxDbContext ctx) => new(ctx);

    private static Aplicacao NovaAplicacao() => new()
    {
        Aplicacaoid = Guid.NewGuid().ToString("N"),
        Nome        = "App Teste",
        Datainicio  = DateTime.UtcNow,
        Isactive    = true
    };

    private static Area NovaAreaHome(string aplicacaoid) => new()
    {
        Areaid      = Guid.NewGuid().ToString("N"),
        Aplicacaoid = aplicacaoid,
        Nome        = "Home"
    };

    [Fact]
    public async Task Lista_RetornaVazia_QuandoTenantNaoExiste()
    {
        using var ctx = _ctxFactory();
        (await Repo(ctx).ListaAsync(Guid.NewGuid().ToString("N")))
        .Should().BeEmpty();

    }

    [Fact]
    public async Task Criar_E_BuscaPorId_RetornaAplicacao()
    {
        using var ctx = _ctxFactory();
        var app       = NovaAplicacao();
        await Repo(ctx).CriarAsync(app, NovaAreaHome(app.Aplicacaoid!));

        using var ctx2 = _ctxFactory();
        (await Repo(ctx2).BuscaPorIdAsync(app.Aplicacaoid!))
            .Should().NotBeNull()
            .And.Match<Aplicacao>(a => a.Aplicacaoid == app.Aplicacaoid);
    }

    [Fact]
    public async Task Lista_FiltraPorTenant()
    {
        using var ctx = _ctxFactory();
        var app1      = NovaAplicacao();
        var app2      = NovaAplicacao();
        await Repo(ctx).CriarAsync(app1, NovaAreaHome(app1.Aplicacaoid!));
        await Repo(ctx).CriarAsync(app2, NovaAreaHome(app2.Aplicacaoid!));

        using var ctx2 = _ctxFactory();
        var resultado  = await Repo(ctx2).ListaAsync(app1.Aplicacaoid);
        resultado.Should().ContainSingle(a => a.Aplicacaoid == app1.Aplicacaoid);
        resultado.Should().NotContain(a => a.Aplicacaoid == app2.Aplicacaoid);
    }

    [Fact]
    public async Task Atualizar_PersisteMudancas()
    {
        using var ctx = _ctxFactory();
        var app       = NovaAplicacao();
        await Repo(ctx).CriarAsync(app, NovaAreaHome(app.Aplicacaoid!));

        using var ctx2 = _ctxFactory();
        var salvo      = await Repo(ctx2).BuscaPorIdAsync(app.Aplicacaoid!)!;
       
        if(salvo != null)
            salvo.Nome     = "Nome Alterado";

        await Repo(ctx2).AtualizarAsync(salvo);

        using var ctx3 = _ctxFactory();
        (await Repo(ctx3).BuscaPorIdAsync(app.Aplicacaoid!))!.Nome.Should().Be("Nome Alterado");
    }

    [Fact]
    public async Task AlterarStatus_DesativaAplicacao()
    {
        using var ctx = _ctxFactory();
        var app       = NovaAplicacao();
        await Repo(ctx).CriarAsync(app, NovaAreaHome(app.Aplicacaoid!));
        using var ctx2 = _ctxFactory();
        var salvo      = (await Repo(ctx2).ListaAsync(app.Aplicacaoid!)!).First();
        await Repo(ctx2).AlterarStatusAsync(salvo, false);

        using var ctx3  = _ctxFactory();
        var resultado   = await Repo(ctx3).BuscaPorIdAsync(app.Aplicacaoid!)!;
        resultado.Isactive.Should().Be(false);
        resultado.Datafinal.Should().NotBeNull();
    }

    [Fact]
    public async Task Remover_ExcluiAplicacao()
    {
        using var ctx = _ctxFactory();
        var app       = NovaAplicacao();
        await Repo(ctx).CriarAsync(app, NovaAreaHome(app.Aplicacaoid!));    
        using var ctx2 = _ctxFactory();
        var salvo      = await Repo(ctx2).BuscaPorIdAsync(app.Aplicacaoid!)!;
        await Repo(ctx2).RemoverAsync(salvo);

        using var ctx3 = _ctxFactory();
        (await Repo(ctx3).BuscaPorIdAsync(app.Aplicacaoid!)).Should().BeNull();
    }
}
