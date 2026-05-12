# MultiplAI — CMS Multi-Tenant com Page Builder IA
![.NET](https://img.shields.io/badge/.NET-6-blue)
![AI](https://img.shields.io/badge/AI-LLM-green)


Sistema de gestão de conteúdo (CMS) multi-tenant desenvolvido com ASP.NET Core 6, Angular 15, Entity Framework Core 7 e SQL Server.

## Funcionalidades

### Core
- **Multi-tenancy** — cada tenant (empresa/aplicação) tem suas áreas, conteúdos, produtos, categorias e usuários isolados
- **Autenticação JWT** — login, grupos, vínculos usuário × módulo (controle de acesso granular)
- **Dashboard** — visão geral por tenant

### Page Builder com IA
- Geração de layouts de página via **Gemini (Google)** ou **Claude (Anthropic)**
- Catálogo de blocos reutilizáveis gerenciado no banco de dados
- Templates de layout: salvar, carregar e definir padrão por tipo
- Cache de respostas da IA com TTL configurável
- Limite diário de gerações por tenant (com suporte a chave própria)
- **Integração Unsplash**: imagens resolvidas automaticamente nos blocos visuais

### Blocos disponíveis
| Bloco | Descrição |
|---|---|
| `hero` | Banner principal com título, subtítulo e botão |
| `hero-cta` | Hero com campo de captura de e-mail |
| `texto` | Seção de texto rico |
| `banner-imagem` | Imagem larga com link |
| `lista-conteudos` | Grade de conteúdos de uma área |
| `lista-produtos` | Grade de produtos com imagem e preço |
| `categorias` | Exibição de categorias em badges/cards |
| `destaques` | Cards com ícone, título e descrição |
| `faq` | Acordeão de perguntas e respostas |
| `formulario` | Formulário dinâmico embutido |
| `prova-social` | Depoimentos com estrelas e cargo |
| `video` | Embed YouTube/Vimeo (conversão automática de URL) |
| `contador` | Contador regressivo em tempo real |
| `menu-navegacao` | Navbar global com links para as áreas |
| `rodape` | Rodapé com endereço, redes sociais e copyright |

### Site Público
- Renderização pública via slug: `/s/{slug}` e `/s/{slug}/{area}`
- Modo preview autenticado: `/preview/{id}`
- Roteamento entre áreas sem recarregar a página (Angular Router)
- Blocos enriquecidos server-side com dados do tenant

### Admin
- Seletor de tenant: admin filtra todas as listas por tenant sem sair da sessão
- Formulários dinâmicos com form builder e inbox de respostas
- Produtos com atributos, opções e galeria de imagens
- Hierarquia de categorias
- FAQ/Wiki vinculado a formulários

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core 6, C# |
| ORM | Entity Framework Core 7 |
| Banco de dados | SQL Server (suporte a PostgreSQL) |
| Frontend | Angular 15, Bootstrap 5, TypeScript |
| Auth | JWT (Bearer Token) |
| IA | Google Gemini / Anthropic Claude |
| Imagens | Unsplash API |

## Configuração

### 1. Banco de dados
Execute os scripts SQL na pasta `CMSXDB/` na seguinte ordem:
```
cmsxDB.sqlserver.sql      — estrutura principal
cmsxDB.seed.sql           — dados iniciais
cmsxDB.grupos.sql         — grupos e permissões
cmsxDB.pagebuilder.sql    — catálogo de blocos
cmsxDB.ia.sql             — tabelas de IA (cache, config, uso)
cmsxDB.layout_template.sql — templates de layout
cmsxDB.menu_navegacao.sql  — bloco menu-navegacao
cmsxDB.novos_blocos.sql   — blocos adicionais
```

### 2. appsettings
Copie `appsettings.json` como `appsettings.Development.json` e preencha:
```json
{
  "Jwt": { "Key": "sua-chave-secreta-minimo-32-chars" },
  "AgentIA": {
    "Gemini": { "ApiKey": "sua-gemini-api-key" }
  },
  "Unsplash": { "AccessKey": "sua-unsplash-access-key" },
  "ConnectionStrings": {
    "SqlServer": "Server=...;Database=cmsxDB;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

### 3. Executar
```bash
cd CMSX/CMSUI
dotnet run
```
A aplicação sobe em `https://localhost:44455` (Angular + API integrados via SPA Proxy).

## Estrutura do projeto

```
CMSX/
├── CMSUI/               # Projeto principal (ASP.NET Core + Angular)
│   ├── Controllers/     # API Controllers
│   ├── Models/          # Entidades EF Core + DbContext
│   ├── Services/        # Agentes de IA (Gemini, Anthropic)
│   └── ClientApp/       # Angular 15 SPA
│       └── src/app/
│           ├── page-builder/   # Page Builder com IA
│           ├── site/           # Renderizador de site público
│           └── ...             # Demais módulos admin
├── CMSXData/            # Modelos de dados (biblioteca)
├── CMSXDAO/             # Repositórios (DAL)
├── cmsBLLCore/          # Regras de negócio (BLL)
├── ICMSX/               # Interfaces
└── CMSXDB/              # Scripts SQL
```

## Licença

Este projeto é um repositório de estudos e aprendizado. Sinta-se livre para explorar o código.
