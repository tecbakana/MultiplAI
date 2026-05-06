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
    public void Lista_RetornaVazia_QuandoTenantNaoExiste()
    {
        using var ctx = _ctxFactory();
        Repo(ctx).Lista(Guid.NewGuid().ToString("N")).Should().BeEmpty();
    }

    [Fact]
    public void Criar_E_BuscaPorId_RetornaAplicacao()
    {
        using var ctx = _ctxFactory();
        var app       = NovaAplicacao();
        Repo(ctx).Criar(app, NovaAreaHome(app.Aplicacaoid!));

        using var ctx2 = _ctxFactory();
        Repo(ctx2).BuscaPorId(app.Aplicacaoid!)
            .Should().NotBeNull()
            .And.Match<Aplicacao>(a => a.Aplicacaoid == app.Aplicacaoid);
    }

    [Fact]
    public void Lista_FiltraPorTenant()
    {
        using var ctx = _ctxFactory();
        var app1      = NovaAplicacao();
        var app2      = NovaAplicacao();
        Repo(ctx).Criar(app1, NovaAreaHome(app1.Aplicacaoid!));
        Repo(ctx).Criar(app2, NovaAreaHome(app2.Aplicacaoid!));

        using var ctx2 = _ctxFactory();
        var resultado  = Repo(ctx2).Lista(app1.Aplicacaoid);

        resultado.Should().ContainSingle(a => a.Aplicacaoid == app1.Aplicacaoid);
        resultado.Should().NotContain(a => a.Aplicacaoid == app2.Aplicacaoid);
    }

    [Fact]
    public void Atualizar_PersisteMudancas()
    {
        using var ctx = _ctxFactory();
        var app       = NovaAplicacao();
        Repo(ctx).Criar(app, NovaAreaHome(app.Aplicacaoid!));

        using var ctx2 = _ctxFactory();
        var salvo      = Repo(ctx2).BuscaPorId(app.Aplicacaoid!)!;
        salvo.Nome     = "Nome Alterado";
        Repo(ctx2).Atualizar(salvo);

        using var ctx3 = _ctxFactory();
        Repo(ctx3).BuscaPorId(app.Aplicacaoid!)!.Nome.Should().Be("Nome Alterado");
    }

    [Fact]
    public void AlterarStatus_DesativaAplicacao()
    {
        using var ctx = _ctxFactory();
        var app       = NovaAplicacao();
        Repo(ctx).Criar(app, NovaAreaHome(app.Aplicacaoid!));

        using var ctx2 = _ctxFactory();
        var salvo      = Repo(ctx2).BuscaPorId(app.Aplicacaoid!)!;
        Repo(ctx2).AlterarStatus(salvo, false);

        using var ctx3  = _ctxFactory();
        var resultado   = Repo(ctx3).BuscaPorId(app.Aplicacaoid!)!;
        resultado.Isactive.Should().Be(false);
        resultado.Datafinal.Should().NotBeNull();
    }

    [Fact]
    public void Remover_ExcluiAplicacao()
    {
        using var ctx = _ctxFactory();
        var app       = NovaAplicacao();
        Repo(ctx).Criar(app, NovaAreaHome(app.Aplicacaoid!));

        using var ctx2 = _ctxFactory();
        var salvo      = Repo(ctx2).BuscaPorId(app.Aplicacaoid!)!;
        Repo(ctx2).Remover(salvo);

        using var ctx3 = _ctxFactory();
        Repo(ctx3).BuscaPorId(app.Aplicacaoid!).Should().BeNull();
    }
}
