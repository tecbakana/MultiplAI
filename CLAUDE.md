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
- **NUNCA** usar `using CMSXData.Models` em controllers — a única referência de dados permitida em controllers é `using ICMSX`
- **NUNCA** instanciar entidades de banco (`new Modulo { }`, `new Produto { }`, etc.) fora de `CMSXRepo` — construção de entidade é responsabilidade exclusiva do repositório
- **NUNCA** definir métodos de criação/atualização em interfaces com entidades como parâmetro de entrada (`CriarAsync(Modulo m)`) — use DTOs definidos em `ICMSX` (`CriarAsync(ModuloInput input)`); o repositório constrói a entidade internamente
- **NUNCA** expor `IQueryable<T>` fora do repositório — LINQ to Entities é vazamento de estrutura
- **NUNCA** tomar atalhos para "fazer funcionar" em detrimento da arquitetura
- **NUNCA** usar `.ToString()` dentro de expressões LINQ/EF Core (quebra no PostgreSQL)
- **NUNCA** usar `DateTime.Now` em qualquer campo que será persistido no banco (dentro ou fora de query) — sempre `DateTime.UtcNow`. Um valor `Kind=Local` grava sem erro no SQL Server e falha explicitamente no PostgreSQL.
- **NUNCA** implementar antes de criar a interface em ICMSX e ter aprovação
- **NUNCA** expor endpoint que aceita query parameter com ID externo (`usuarioid`, `aplicacaoid`, etc.) sem verificar a autorização do caller primeiro — ausência de gate é BOLA/IDOR (OWASP API Security #1), violação de segurança bloqueante
- **NUNCA** remover uma entidade sem verificar se há registros relacionados em outras tabelas: ou deletar explicitamente os vínculos antes, ou confirmar no schema que CASCADE DELETE está configurado — omissão gera dados órfãos silenciosos

---

## OBRIGAÇÕES

- Toda persistência via interface definida em `ICMSX`
- Se a interface não existir: **criar em ICMSX primeiro**, apresentar ao responsável, depois implementar em `CMSXRepo`
- Queries encapsuladas em métodos semânticos: `Lista(string aplicacaoid)`, `BuscaPorId(Guid id)` — nunca expor predicados
- DTOs declarados como `record` em `ICMSX`, no mesmo arquivo da interface ou em arquivo próprio. Convenção de naming:
  - **Entrada** (dados que chegam ao repositório via POST/PUT): sufixo `Input` → `ModuloInput`, `ProdutoInput`
  - **Saída** (dados retornados ao cliente quando a entidade não é suficiente): nome semântico descritivo → `LoginResultado`, `DashboardTotais`, `ProdutoPublico`
  - Nunca usar sufixo genérico `Dto` — o nome deve comunicar propósito
- Parâmetros de busca chegam ao repositório já no tipo correto da coluna — o repositório não faz conversão de tipo, apenas usa o parâmetro recebido. A camada que chama o repositório (controller ou service) é responsável por converter antes de chamar.
- Validação de escopo de tenant **dentro do repositório**, não no controller
- Campos sensíveis — nunca logar, nunca incluir em DTOs de resposta sem necessidade explícita: `Senha`, `Pagsegurotoken`, `Mailpassword`, `AccessToken`, `RefreshToken`, `ClientSecret`, `ApiKey`, `Secret`
- Reportar como **impeditivo** se houver ambiguidade sobre qual interface criar ou qual comportamento preservar

---

## Ground Truth — Padrão arquitetural válido

**A única referência válida é o módulo `Modulo`:**
- `ICMSX/IModuloRepositorio.cs` + `ICMSX/ModuloInput.cs`
- `CMSXRepo/ModuloRepositorio.cs`
- `CMSAPI/Controllers/ModulosController.cs`

**RAG é válido para descoberta de estrutura:**
- Ler `CMSXData.Models` para entender campos de entidades e montar DTOs
- Ler `CmsxDbContext` para descobrir DbSets disponíveis e nomes de tabelas
- Ler `DependencyInjectionExtensions.cs` para saber onde registrar o novo repositório
- Ler rotas e atributos de outros controllers para entender convenções de URL

**RAG não é válido para reúso de padrão de implementação.** Qualquer código encontrado no repositório que:
- utilize `DbContext` fora da camada `CMSXRepo`/`CMSXData`
- realize `.ToString()`, `Convert.To*()` ou qualquer conversão dentro de `IQueryable`
- injete `CmsxDbContext` em controllers ou services
- instancie entidades fora de `CMSXRepo`
- use `using CMSXData.Models` em controllers

**não deve ser replicado — é legado com débito técnico registrado.** Para implementar, seguir o padrão canônico do módulo `Modulo`, não o que o RAG trouxer de outros arquivos.

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
2. Reconstrua do zero seguindo o módulo `Modulo` (`IModuloRepositorio` → `ModuloRepositorio` → `ModulosController`)
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
// CERTO — leitura exposta pela interface: sempre AsNoTracking
public async Task<IEnumerable<Modulo>> ListaTodosAsync() =>
    await _db.Modulos.AsNoTracking().OrderBy(m => m.Posicao).ToListAsync();

public async Task<Modulo?> BuscaPorIdAsync(string moduloid) =>
    await _db.Modulos.AsNoTracking().FirstOrDefaultAsync(m => m.Moduloid == moduloid);

// CERTO — busca interna de escrita (dentro do próprio método de atualização): sem AsNoTracking
public async Task<bool> AtualizarAsync(string id, ModuloInput input)
{
    var modulo = await _db.Modulos.FirstOrDefaultAsync(m => m.Moduloid == id); // tracking ativo
    if (modulo == null) return false;
    modulo.Nome = input.Nome;
    await _db.SaveChangesAsync();
    return true;
}
```

**Regra:** todo método público da interface que retorna dados usa `AsNoTracking`. Buscas internas dentro de métodos de escrita (que chamam `SaveChangesAsync` na sequência) não usam `AsNoTracking` — o tracking é necessário para o EF Core detectar as mudanças.

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

**Leia os arquivos reais antes de implementar — eles são a fonte de verdade:**
- `ICMSX/IModuloRepositorio.cs` + `ICMSX/ModuloInput.cs`
- `CMSXRepo/ModuloRepositorio.cs`
- `CMSAPI/Controllers/ModulosController.cs`

### DTO de entrada (ICMSX)

```csharp
// ICMSX/ModuloInput.cs
namespace ICMSX;

public record ModuloInput(string? Nome, string? Url, int? Posicao);
```

### Interface (ICMSX)

```csharp
// ICMSX/IModuloRepositorio.cs
using CMSXData.Models;

namespace ICMSX;

public interface IModuloRepositorio
{
    Task<IEnumerable<Modulo>> ListaTodosAsync();
    Task<IEnumerable<Modulo>> ListaPorAplicacaoAsync(string aplicacaoid);
    Task<IEnumerable<Modulo>> ListaPorUsuarioAsync(string usuarioid);
    Task<Modulo?> BuscaPorIdAsync(string moduloid);
    Task<string> CriarAsync(ModuloInput input);              // ← retorna o ID gerado
    Task<bool> AtualizarAsync(string id, ModuloInput input); // ← retorna false se não encontrado
    Task<bool> RemoverAsync(string id);                       // ← retorna false se não encontrado
}
```

### Implementação (CMSXRepo)

```csharp
// CMSXRepo/ModuloRepositorio.cs
using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;  // ← permitido APENAS aqui

namespace CMSXRepo;

public class ModuloRepositorio : BaseRepositorio, IModuloRepositorio
{
    public ModuloRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Modulo>> ListaTodosAsync() =>
        await _db.Modulos.AsNoTracking().OrderBy(m => m.Posicao).ToListAsync();

    public async Task<Modulo?> BuscaPorIdAsync(string moduloid) =>
        await _db.Modulos.AsNoTracking().FirstOrDefaultAsync(m => m.Moduloid == moduloid);

    public async Task<string> CriarAsync(ModuloInput input)
    {
        var modulo = new Modulo           // ← entidade construída AQUI, nunca no controller
        {
            Moduloid = Guid.NewGuid().ToString(),
            Nome     = input.Nome,
            Url      = input.Url,
            Posicao  = input.Posicao
        };
        _db.Modulos.Add(modulo);
        await _db.SaveChangesAsync();
        return modulo.Moduloid;
    }

    public async Task<bool> AtualizarAsync(string id, ModuloInput input)
    {
        var modulo = await _db.Modulos.FirstOrDefaultAsync(m => m.Moduloid == id);
        if (modulo == null) return false;  // ← tracking implícito: sem AsNoTracking

        modulo.Nome    = input.Nome;
        modulo.Url     = input.Url;
        modulo.Posicao = input.Posicao;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoverAsync(string id)
    {
        var modulo = await _db.Modulos.FirstOrDefaultAsync(m => m.Moduloid == id);
        if (modulo == null) return false;  // ← tracking implícito: sem AsNoTracking

        _db.Modulos.Remove(modulo);
        await _db.SaveChangesAsync();
        return true;
    }
}
```

### Controller (CMSAPI)

```csharp
// CMSAPI/Controllers/ModulosController.cs
using ICMSX;  // ← única referência de dados permitida no controller
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ModulosController : Controller
{
    private readonly IModuloRepositorio _repo;
    public ModulosController(IModuloRepositorio repo) { _repo = repo; }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? usuarioid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        // usuarioid externo: exclusivo para admin — não-admin não pode enumerar outro usuário
        if (!string.IsNullOrEmpty(usuarioid))
        {
            if (!acessoTotal) return Forbid();  // ← BOLA/IDOR: gate obrigatório
            return Ok(await _repo.ListaPorUsuarioAsync(usuarioid));
        }
        if (acessoTotal)
            return Ok(await _repo.ListaTodosAsync());
        // não-admin: userid vem do JWT, nunca do cliente
        var claimUserId = User.FindFirstValue("userid");
        return Ok(await _repo.ListaPorUsuarioAsync(claimUserId!));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ModuloInput input)
    {
        var (acessoTotal, _) = UserContext();
        if (!acessoTotal) return Forbid();

        var moduloid = await _repo.CriarAsync(input);  // ← DTO passa direto; repositório constrói a entidade
        return Ok(new { moduloid });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] ModuloInput input)
    {
        var (acessoTotal, _) = UserContext();
        if (!acessoTotal) return Forbid();

        var atualizado = await _repo.AtualizarAsync(id, input);
        if (!atualizado) return NotFound();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (acessoTotal, _) = UserContext();
        if (!acessoTotal) return Forbid();

        var removido = await _repo.RemoverAsync(id);  // ← repositório busca e remove internamente
        if (!removido) return NotFound();
        return NoContent();
    }
}
```

### Registro de DI (CMSXRepo/DependencyInjectionExtensions.cs)

```csharp
services.AddScoped<IModuloRepositorio, ModuloRepositorio>();
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

## Erros e problemas identificados durante a execução de uma task

O escopo da dev-request define o que deve ser implementado, mas a qualidade da implementação como um todo é prioridade superior. Ao identificar um problema fora do escopo:

**1. É impeditivo** (bloqueia a implementação atual, gera risco de regressão ou impede o correto funcionamento do que está sendo entregue):
→ Corrigir imediatamente no mesmo PR/commit e registrar no campo `resultado` da dev-request o que foi corrigido e por quê.

**2. Não é impeditivo** (débito técnico, melhoria, aviso de qualidade):
→ Não corrigir agora. Sinalizar para criação de nova dev-request descrevendo o problema, o arquivo/linha, o risco de deixar e a ação necessária.

**Nunca:**
- ignorar silenciosamente um problema identificado
- deixar comentário TODO sem dev-request associada
- abrir impeditivo na dev-request atual por problema não impeditivo (isso trava a entrega sem necessidade)

## Débito técnico e vulnerabilidades

- Antes de encerrar: `dotnet list package --vulnerable` — atualizar tudo ou nova dev-request com risco detalhado.

## Testes antes de encerrar

Após qualquer implementação que altere `CMSXRepo` ou `CMSXData`, execute os testes de integração:

```bash
dotnet test CMSX.Tests/CMSX.Tests.csproj --filter "Integration"
```

- Se houver falhas: interprete cada erro, corrija o código e rode novamente até passar.
- Só encerre a tarefa após os testes passarem — testes vermelhos não são entregáveis.
- O revisor (Opus) verifica conformidade arquitetural; você (Sonnet) é responsável por garantir que os testes passam antes de sinalizar conclusão.

### Cobertura obrigatória para novas classes de repositório

Toda nova classe criada em `CMSXRepo` **exige avaliação explícita** da necessidade de testes de integração com Testcontainers. A avaliação deve responder:

1. O repositório persiste ou lê do banco? → **criar testes de integração**
2. Há comportamento implícito do EF Core sendo explorado (tracking, lazy loading, cascade)? → **obrigatório cobrir com teste**
3. O repositório é puramente em memória ou delega para outro serviço? → testes unitários bastam

Se a criação dos testes estiver fora do escopo da task atual, abrir task separada descrevendo os cenários a cobrir antes de encerrar. Nunca entregar repositório novo sem ao menos registrar a task de cobertura.

---

## Se estiver em dúvida

Pare. Releia este arquivo. Se a dúvida persistir, sinalize como **impeditivo** e aguarde instrução.
