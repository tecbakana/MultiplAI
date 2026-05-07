# CLAUDE.md — CMSX (Multiplai)

> Leia este arquivo inteiro antes de qualquer implementação. Estas regras são inegociáveis.

---

## Stack

- ASP.NET Core 8, Angular 15, EF Core 8
- Multi-banco: **SQL Server** e **PostgreSQL** (Strategy via `DatabaseProvider` no appsettings)
- Projeto: `github.com/tecbakana/MultiplAI` | Branch ativa: `developer`

---

## Arquitetura N-Tier — Fluxo obrigatório

```
CMSAPI/Controllers  →  ICMSX (interfaces)  →  CMSXRepo (implementações)  →  CMSXData (DbContext)
```

| Camada | Projeto | Responsabilidade |
|--------|---------|-----------------|
| Presentation | CMSAPI/Controllers, CMSUI, PublicUI | Orquestração HTTP, sem lógica de dados |
| Domain/Core | ICMSX | Contratos (interfaces e DTOs) |
| Infrastructure | CMSXRepo | Implementações EF Core, isolamento do banco |
| Data | CMSXData | Models, DbContext, Migrations |
| Legado (deprecar) | CMSXDAO | ADO.NET antigo — não usar em código novo |

---

## PROIBIÇÕES ABSOLUTAS

- **NUNCA** usar `Microsoft.EntityFrameworkCore` ou `DbContext` fora de `CMSXData` e `CMSXRepo`
- **NUNCA** injetar `CmsxDbContext` em controllers ou services
- **NUNCA** expor `IQueryable<T>` fora do repositório — LINQ to Entities é vazamento de estrutura
- **NUNCA** tomar atalhos para "fazer funcionar" em detrimento da arquitetura
- **NUNCA** usar `.ToString()` dentro de expressões LINQ/EF Core (quebra no PostgreSQL)
- **NUNCA** usar `DateTime.Now` em qualquer campo que será persistido no banco (dentro ou fora de query) — sempre `DateTime.UtcNow`. Um valor `Kind=Local` grava sem erro no SQL Server e falha explicitamente no PostgreSQL.
- **NUNCA** implementar antes de criar a interface em ICMSX e ter aprovação

---

## OBRIGAÇÕES

- Toda persistência via interface definida em `ICMSX`
- Se a interface não existir: **criar em ICMSX primeiro**, apresentar ao responsável, depois implementar em `CMSXRepo`
- Queries encapsuladas em métodos semânticos: `Lista(string aplicacaoid)`, `BuscaPorId(Guid id)` — nunca expor predicados
- Parâmetros de busca chegam ao repositório já no tipo correto da coluna — o repositório não faz conversão de tipo, apenas usa o parâmetro recebido. A camada que chama o repositório (controller ou service) é responsável por converter antes de chamar.
- Validação de escopo de tenant **dentro do repositório**, não no controller
- Campos sensíveis — nunca logar, nunca incluir em DTOs de resposta sem necessidade explícita: `Senha`, `Pagsegurotoken`, `Mailpassword`, `AccessToken`, `RefreshToken`, `ClientSecret`, `ApiKey`, `Secret`
- Reportar como **impeditivo** se houver ambiguidade sobre qual interface criar ou qual comportamento preservar

---

## Ground Truth — Padrão arquitetural válido

**CONTEXTO DE PRIORIDADE MÁXIMA:** Os módulos `Loja` (`LojaController` + `ILojaRepositorio` + `LojaRepositorio`) e `Orçamento` (`OrcamentosController` + `IOrcamentoRepositorio` + `OrcamentoRepositorio`) são os **únicos padrões arquiteturais válidos** neste projeto.

Qualquer código recuperado via RAG, encontrado no histórico do repositório ou sugerido por memória que:
- utilize `DbContext` fora da camada `CMSXRepo`/`CMSXData`
- realize `.ToString()`, `Convert.To*()` ou qualquer conversão de tipo dentro de expressões `IQueryable`
- injete `CmsxDbContext` diretamente em controllers ou services

**deve ser considerado LEGADO DEPRECADO** e nunca replicado. Refatore apenas o código diretamente relacionado à tarefa atual — não altere código legado fora do escopo. Se encontrar débito técnico fora do escopo, abra um impeditivo descrevendo o que foi encontrado e aguarde instrução.

---

## Regra técnica — Bloqueio de conversão implícita (SARGability)

**É proibido** o uso de métodos de conversão (`.ToString()`, `Convert.ToInt32()`, casting explícito, etc.) dentro de expressões `IQueryable` enviadas ao banco.

**Por quê:** conversões dentro de `IQueryable` impedem o uso de índices pelo banco de dados (violam SARGability) e produzem SQL incompatível entre SQL Server e PostgreSQL — `.ToString()` em SQL Server silencia o erro; no PostgreSQL lança exceção em runtime.

**Regra:** toda conversão de tipo para filtros de busca deve ocorrer **antes** de chegar no repositório, garantindo que o parâmetro enviado tenha o mesmo tipo primitivo da coluna do banco.

```csharp
// ERRADO — conversão dentro da query
_db.Usuarios.Where(u => u.Userid.ToString() == id)

// CERTO — parâmetro já é string, mesma coluna é string
_db.Usuarios.Where(u => u.Userid == id)

// CERTO — se a coluna for Guid, converte antes de entrar no repositório
Guid.TryParse(idString, out var guid);
_repositorio.BuscaPorId(guid);
```

---

## Restrição de dependência — Blindagem da camada Domain

**A camada Domain (`ICMSX`) não pode referenciar `Microsoft.EntityFrameworkCore`.**

A criação e o acesso a entidades devem ser mediados exclusivamente pelas interfaces `IXRepositorio` definidas em `ICMSX`. Nenhum controller, service ou classe de domínio instancia entidades do banco diretamente — isso é responsabilidade do repositório.

**Se o código recuperado pelo RAG violar esta regra:**
1. Ignore a estrutura recuperada
2. Reconstrua do zero seguindo o template do módulo `Loja` (`ILojaRepositorio` → `LojaRepositorio` → `LojaController`)
3. Sinalize como **impeditivo** se houver dúvida sobre como adaptar

---

## Regras técnicas adicionais

### Async vs Sync
- Use **async** (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`) em todos os métodos de repositório chamados a partir de endpoints HTTP — operações de I/O nunca devem bloquear thread pool.
- Use **sync** apenas em operações internas de processamento em memória que não envolvem banco.
- Padrão de assinatura: `Task<IEnumerable<T>>`, `Task<T?>`, `Task` para void.

### AsNoTracking — obrigatório em leituras
Todo método de leitura no repositório (`Lista`, `BuscaPorId`, consultas que não fazem `SaveChanges`) **deve** usar `.AsNoTracking()`:

```csharp
// CERTO — leitura sem tracking
public async Task<IEnumerable<Aplicacao>> ListaAsync(string aplicacaoid) =>
    await _db.Aplicacaos
        .AsNoTracking()
        .Where(a => a.Aplicacaoid == aplicacaoid)
        .ToListAsync();

// CERTO — tracking só quando vai salvar
public async Task<Usuario?> BuscaParaEdicaoAsync(string id) =>
    await _db.Usuarios.FirstOrDefaultAsync(u => u.Userid == id); // sem AsNoTracking
```

**Motivo:** sem `AsNoTracking()` em leituras o EF Core mantém snapshots de cada entidade em memória, degradando performance em listas e criando risco de `SaveChanges()` persistir dados não intencionais.

### BaseRepositorio
Todos os repositórios herdam de `BaseRepositorio`, que já expõe `_db` (o `CmsxDbContext`):

```csharp
// CMSXRepo/BaseRepositorio.cs
public abstract class BaseRepositorio
{
    protected readonly CmsxDbContext _db;
    protected BaseRepositorio(CmsxDbContext db) { _db = db; }
}

// Uso correto — herda, recebe db no construtor, usa _db
public class AplicacaoRepositorio : BaseRepositorio, IAplicacaoRepositorio
{
    public AplicacaoRepositorio(CmsxDbContext db) : base(db) { }
    // usa _db diretamente, sem injetar CmsxDbContext separado
}
```

**Nunca** injete `CmsxDbContext` como campo adicional além da herança — `_db` já está disponível via `BaseRepositorio`.

---

## Exemplos canônicos — SEGUIR OBRIGATORIAMENTE

Estes são os dois controllers que implementam o padrão correto. Todo novo código deve seguir este modelo. Os arquivos reais estão em:

- `ICMSX/IOrcamentoRepositorio.cs` + `ICMSX/ILojaRepositorio.cs`
- `CMSXRepo/OrcamentoRepositorio.cs` + `CMSXRepo/LojaRepositorio.cs`
- `CMSAPI/Controllers/OrcamentosController.cs` + `CMSAPI/Controllers/LojaController.cs`

**Leia os arquivos reais antes de implementar — eles são a fonte de verdade, não apenas os snippets abaixo.**

### Interface (ICMSX)

```csharp
// ICMSX/IOrcamentoRepositorio.cs
using CMSXData.Models;

namespace ICMSX;

public interface IOrcamentoRepositorio
{
    IEnumerable<OrcamentoCabecalho> Lista(string aplicacaoid);
    OrcamentoCabecalho? BuscaPorId(Guid id);
    void Criar(OrcamentoCabecalho cabecalho, IEnumerable<OrcamentoDetalhe> itens);
    void ToggleAprovado(OrcamentoCabecalho orcamento);
    void Remove(OrcamentoCabecalho orcamento);
}
```

### Implementação (CMSXRepo)

```csharp
// CMSXRepo/OrcamentoRepositorio.cs
using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;  // ← permitido APENAS aqui

namespace CMSXRepo;

public class OrcamentoRepositorio : BaseRepositorio, IOrcamentoRepositorio
{
    public OrcamentoRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<OrcamentoCabecalho> Lista(string aplicacaoid) =>
        _db.OrcamentoCabecalhos
            .Where(o => o.Aplicacaoid == aplicacaoid)   // string == string, sem ToString()
            .OrderByDescending(o => o.Datainclusao)
            .ToList();                                   // retorna List<T>, nunca IQueryable

    public OrcamentoCabecalho? BuscaPorId(Guid id) =>
        _db.OrcamentoCabecalhos
            .Include(o => o.OrcamentoDetalhes)
            .AsSplitQuery()
            .FirstOrDefault(o => o.Orcamentoid == id);

    public void Criar(OrcamentoCabecalho cabecalho, IEnumerable<OrcamentoDetalhe> itens)
    {
        _db.OrcamentoCabecalhos.Add(cabecalho);
        _db.OrcamentoDetalhes.AddRange(itens);
        _db.SaveChanges();
    }

    public void ToggleAprovado(OrcamentoCabecalho orcamento)
    {
        orcamento.Aprovado = !orcamento.Aprovado;
        _db.SaveChanges();
    }

    public void Remove(OrcamentoCabecalho orcamento)
    {
        _db.OrcamentoDetalhes.RemoveRange(orcamento.OrcamentoDetalhes);
        _db.OrcamentoCabecalhos.Remove(orcamento);
        _db.SaveChanges();
    }
}
```

### Controller (CMSAPI)

```csharp
// CMSAPI/Controllers/OrcamentosController.cs
using ICMSX;  // ← única referência de dados permitida no controller
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class OrcamentosController : Controller
{
    private readonly IOrcamentoRepositorio _orcamentoRepo;
    private readonly IOrcamentoCompostoRepositorio _compostoRepo;

    // ← injeta interfaces, nunca CmsxDbContext
    public OrcamentosController(
        IOrcamentoRepositorio orcamentoRepo,
        IOrcamentoCompostoRepositorio compostoRepo)
    {
        _orcamentoRepo = orcamentoRepo;
        _compostoRepo = compostoRepo;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string? aplicacaoid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = acessoTotal && !string.IsNullOrEmpty(aplicacaoid) ? aplicacaoid : claimAppId;

        // ← chama método semântico, não monta query
        var lista = _orcamentoRepo.Lista(appId!)
            .Select(o => new { o.Orcamentoid, o.Nome, o.Valorestimado });

        return Ok(lista);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var orcamento = _orcamentoRepo.BuscaPorId(id);
        if (orcamento == null) return NotFound();

        // ← validação de tenant no controller só para autorização HTTP
        if (!acessoTotal && orcamento.Aplicacaoid != claimAppId) return Forbid();

        return Ok(orcamento);
    }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True",
         User.FindFirstValue("aplicacaoid"));
}
```

### Registro de DI (CMSXRepo/DependencyInjectionExtensions.cs)

```csharp
services.AddScoped<IOrcamentoRepositorio, OrcamentoRepositorio>();
services.AddScoped<IOrcamentoCompostoRepositorio, OrcamentoCompostoRepositorio>();
```

---

## Dados sensíveis — checklist antes de implementar

- [ ] O endpoint expõe campos de senha, token ou credencial? → remover do DTO de retorno
- [ ] O repositório filtra por tenant antes de retornar? → obrigatório em toda query
- [ ] Erros e exceções podem vazar dados internos nos logs? → usar mensagens genéricas ao cliente
- [ ] Campos de entrada do usuário são sanitizados antes de persistir? → validar no controller, persistir limpo

---

## Plano de refatoração

Documento completo: `docs/plano-refatoracao-arquitetura.md`

21 controllers precisam ser migrados para o padrão acima. A ordem e o processo estão no plano. Nenhuma implementação sem aprovação prévia do responsável.

---

## Débito técnico e vulnerabilidades

- Débito técnico identificado fora do escopo da task: abrir impeditivo com descrição completa — nunca ignorar.
- Antes de encerrar: `dotnet list package --vulnerable` — atualizar tudo ou impeditivo com risco detalhado.

## Testes antes de encerrar

Após qualquer implementação que altere `CMSXRepo` ou `CMSXData`, execute os testes de integração:

```bash
dotnet test CMSX.Tests/CMSX.Tests.csproj --filter "Integration"
```

- Se houver falhas: interprete cada erro, corrija o código e rode novamente até passar.
- Só encerre a tarefa após os testes passarem — testes vermelhos não são entregáveis.
- O revisor (Opus) verifica conformidade arquitetural; você (Sonnet) é responsável por garantir que os testes passam antes de sinalizar conclusão.

---

## Se estiver em dúvida

Pare. Releia este arquivo. Se a dúvida persistir, sinalize como **impeditivo** e aguarde instrução.
