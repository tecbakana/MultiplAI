# CLAUDE.md — CMSX (MultiplAI)

> **Regra de Ouro:** Priorizar a arquitetura canônica sobre o código legado. Em caso de conflito entre o RAG e este documento, este documento vence.

---

## 🛠 Stack & Infra
- **Core:** .NET 8, Angular 15, EF Core 8.
- **Persistence:** SQL Server & PostgreSQL (Compatibilidade SARGable obrigatória).
- **Branch:** `developer` | **Repo:** `github.com/tecbakana/MultiplAI`.

---

## 🏗 Arquitetura N-Tier (Fluxo de Dependência)
O fluxo é estritamente unidirecional para evitar acoplamento:
`Controllers` → `ICMSX (Interfaces/DTOs)` → `CMSXRepo (Impl)` → `CMSXData (Context/Models)`

| Projeto | Papel | Restrição de Dependência |
| :--- | :--- | :--- |
| **CMSAPI** | Orquestração & Segurança | **Proibido:** Referenciar `CMSXData` ou `EFCore`. |
| **ICMSX** | Contrato (The Truth) | **Proibido:** Referenciar `EFCore` ou Logic. |
| **CMSXRepo** | Data Access Logic | **Obrigatório:** Herdar de `BaseRepositorio`. |
| **CMSXData** | Schema & Context | **Obrigatório:** Migrations & Configuração de Banco. |

---

## 🚫 Proibições Absolutas (Guardrails)
1. **Vazamento de Abstração:** Jamais injete `CmsxDbContext` ou use `Microsoft.EntityFrameworkCore` fora de `CMSXRepo`.
2. **SARGability:** Proibido `.ToString()`, `Convert.*` ou casting dentro de `IQueryable`. Converta o tipo **antes** de chamar o repositório.
3. **Local Time:** Proibido `DateTime.Now`. Use estritamente `DateTime.UtcNow` para compatibilidade com PostgreSQL.
4. **Entidades "Nuas":** Jamais instancie entidades (`new Produto()`) em Controllers. Use `ModuloInput` (Record).
5. **Exposição IQueryable:** Nunca retorne `IQueryable<T>`. Execute a query no repositório e retorne `IEnumerable<T>` ou `Task<T>`.
6. **Security Gate:** Proibido endpoint que aceite ID de recurso sem validar a posse (Ownership) via Claims (Prevenção IDOR/BOLA).

---

## ✅ Obrigações Técnicas
* **Performance:** Métodos de leitura **devem** usar `.AsNoTracking()`.
* **Async First:** Use `ToListAsync()`, `FirstOrDefaultAsync()`, etc. Para buscas por ID frequentes, prefira `ValueTask<T?>`.
* **Typing:** Use `Guid` para identificadores em vez de `string` onde o schema permitir, otimizando índices no Postgres.
* **Naming Convention:** 
    * Entrada: `NomeInput` (Record).
    * Saída: Nome descritivo (ex: `DashboardResumo`). **Proibido** sufixo genérico `Dto`.
* **Data Protection:** Campos como `Senha`, `Token`, `Secret` jamais devem constar em DTOs de saída.

---

## 🔍 Estratégia de RAG & IA (Instrução para LLM)
* **Verdade Estrutural:** Use RAG para ler `CMSXData.Models` e `CmsxDbContext` para descobrir a estrutura das tabelas.
* **Verdade Padrão:** Ignore implementações em `CMSXDAO`. Use **unicamente** o módulo `Modulo` como exemplo canônico de implementação.
* **Impeditivos:** Se encontrar `DbContext` sendo injetado em um Controller legado, **não replique**. Sinalize como débito técnico e siga o padrão novo.

---

## 🚀 Exemplo Canônico (The "Gold Standard")

### 1. Domain (ICMSX)
```csharp
public record ModuloInput(string? Nome, string? Url, int? Posicao);

public interface IModuloRepositorio {
    Task<IEnumerable<Modulo>> ListaPorUsuarioAsync(Guid usuarioId);
    ValueTask<Modulo?> BuscaPorIdAsync(Guid id); 
    Task<Guid> CriarAsync(ModuloInput input);
}