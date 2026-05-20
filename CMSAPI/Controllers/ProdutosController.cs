using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using CMSAPI.Services;
using ICMSX;
using CMSXData.Models;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ProdutosController : Controller
    {
        private readonly IProdutosRepositorio _repo;
        private readonly IAgentIAFactory _agentFactory;
        private readonly IWebHostEnvironment _env;
        private readonly IProdutoMaoDeObraRepositorio _moRepo;
        private readonly IProdutoTemplateRepositorio _templateRepo;
        private readonly ISegmentoTenantRepositorio _segmentoRepo;

        public ProdutosController(
            IProdutosRepositorio repo,
            IAgentIAFactory agentFactory,
            IWebHostEnvironment env,
            IProdutoMaoDeObraRepositorio moRepo,
            IProdutoTemplateRepositorio templateRepo,
            ISegmentoTenantRepositorio segmentoRepo)
        {
            _repo = repo;
            _agentFactory = agentFactory;
            _env = env;
            _moRepo = moRepo;
            _templateRepo = templateRepo;
            _segmentoRepo = segmentoRepo;
        }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        // ── CRUD Produto ─────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IEnumerable<Produto>> Get([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var resolvedAppId = acessoTotal
                ? (string.IsNullOrEmpty(aplicacaoid) ? null : aplicacaoid)
                : claimAppId;
            return await _repo.ListaAsync(resolvedAppId);
        }

        public class NovoProdutoDto
        {
            public string? Nome { get; set; }
            public string? Descricao { get; set; }
            public string? Descricacurta { get; set; }
            public string? Detalhetecnico { get; set; }
            public string? Pagsegurokey { get; set; }
            public decimal? Valor { get; set; }
            public string Sku { get; set; } = "";
            public int? Tipo { get; set; }
            public int? Destaque { get; set; }
            public string? Cateriaid { get; set; }
            public string? Aplicacaoid { get; set; }
            public string? UnidadeVenda { get; set; }
        }

        [HttpPost]
        public  async Task<IActionResult> Post([FromBody] NovoProdutoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = new Produto
            {
                Produtoid      = Guid.NewGuid().ToString(),
                Sku            = string.IsNullOrWhiteSpace(dto.Sku) ? Guid.NewGuid().ToString()[..8] : dto.Sku,
                Nome           = dto.Nome,
                Descricao      = dto.Descricao,
                Descricacurta  = dto.Descricacurta,
                Detalhetecnico = dto.Detalhetecnico,
                Pagsegurokey   = dto.Pagsegurokey,
                Valor          = dto.Valor,
                Tipo           = dto.Tipo,
                Destaque       = dto.Destaque,
                Cateriaid      = dto.Cateriaid,
                Aplicacaoid    = acessoTotal ? dto.Aplicacaoid : claimAppId,
                UnidadeVenda   = dto.UnidadeVenda,
                Datainicio     = DateTime.UtcNow
            };
            await _repo.CriarAsync(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public  async Task<IActionResult> Put(string id, [FromBody] NovoProdutoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();

            item.Nome           = dto.Nome;
            item.Descricao      = dto.Descricao;
            item.Descricacurta  = dto.Descricacurta;
            item.Detalhetecnico = dto.Detalhetecnico;
            item.Pagsegurokey   = dto.Pagsegurokey;
            item.Valor          = dto.Valor;
            item.Tipo           = dto.Tipo;
            item.Destaque       = dto.Destaque;
            item.Cateriaid      = dto.Cateriaid;
            item.UnidadeVenda   = dto.UnidadeVenda;
            await _repo.AtualizarAsync(item);
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public  async Task<IActionResult> Delete(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
            await _repo.RemoverAsync(item);
            return Ok();
        }

        // ── Atributos ────────────────────────────────────────────────────────────

        private record OpcaoResponse(string Opcaoid, string? Nome, string? Descricao, int Qtd, int? Estoque, decimal? ValorAdicional);
        private record AtributoResponse(Guid Atributoid, string Nome, string Descricao, int? Ordem, Guid? ParentAtributoId,
            decimal? ValorAdicional, List<OpcaoResponse> Opcoes, List<AtributoResponse> Filhos);

        private List<AtributoResponse> BuildAtributoTree(
            List<Atributo> todos,
            Dictionary<Guid, List<OpcaoResponse>> opcoesPorAtributo,
            Guid? parentId)
        {
            return todos
                .Where(a => a.ParentAtributoId == parentId)
                .OrderBy(a => a.Ordem ?? 0)
                .Select(a =>
                {
                    var filhos = BuildAtributoTree(todos, opcoesPorAtributo, a.Atributoid);
                    var opcoes = filhos.Count == 0 && opcoesPorAtributo.TryGetValue(a.Atributoid, out var ops) ? ops : [];
                    return new AtributoResponse(a.Atributoid, a.Nome, a.Descricao, a.Ordem, a.ParentAtributoId, a.ValorAdicional, opcoes, filhos);
                })
                .ToList();
        }

        [HttpGet("{id}/atributos")]
        public  async Task<IActionResult> GetAtributos(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var arvore = await _repo.BuscaArvoreComOpcoesAsync(id);
            var opcoesPorAtributo = arvore.Opcoes
                .GroupBy(o => o.Atributoid)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(o => new OpcaoResponse(o.Opcaoid, o.Nome, o.Descricao, o.Qtd, o.Estoque, o.ValorAdicional)).ToList()
                );

            return Ok(BuildAtributoTree(arvore.Atributos.ToList(), opcoesPorAtributo, null));
        }

        public class AtributoDto
        {
            public string Nome { get; set; } = "";
            public string? Descricao { get; set; }
            public Guid? ParentAtributoId { get; set; }
            public int? Ordem { get; set; }
            public decimal? ValorAdicional { get; set; }
        }

        [HttpPost("{id}/atributos")]
        public  async Task<IActionResult> PostAtributo(string id, [FromBody] AtributoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            if (dto.ParentAtributoId.HasValue && await _repo.BuscaAtributoAsync(dto.ParentAtributoId.Value) == null)
                return BadRequest("ParentAtributoId não encontrado.");

            var a = new Atributo
            {
                Atributoid       = Guid.NewGuid(),
                Produtoid        = dto.ParentAtributoId.HasValue ? null : id,
                ParentAtributoId = dto.ParentAtributoId,
                Nome             = dto.Nome,
                Descricao        = dto.Descricao ?? "",
                Ordem            = dto.Ordem,
                ValorAdicional   = dto.ValorAdicional
            };
            await _repo.CriarAtributoAsync(a);
            return Ok(new { a.Atributoid, a.Nome, a.Descricao, a.Ordem, a.ParentAtributoId, a.Produtoid });
        }

        [HttpPut("{id}/atributos/{atributoid}")]
        public  async Task<IActionResult> PutAtributo(string id, Guid atributoid, [FromBody] AtributoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var a = await _repo.BuscaAtributoAsync(atributoid);
            if (a == null) return NotFound();

            if (dto.ParentAtributoId.HasValue && dto.ParentAtributoId != a.ParentAtributoId &&
                await _repo.BuscaAtributoAsync(dto.ParentAtributoId.Value) == null)
                return BadRequest("ParentAtributoId não encontrado.");

            a.Nome             = dto.Nome;
            a.Descricao        = dto.Descricao ?? a.Descricao;
            a.Ordem            = dto.Ordem ?? a.Ordem;
            a.ParentAtributoId = dto.ParentAtributoId;
            a.ValorAdicional   = dto.ValorAdicional;
            await _repo.AtualizarAtributoAsync(a);
            return Ok(new { a.Atributoid, a.Nome, a.Descricao, a.Ordem, a.ParentAtributoId, a.Produtoid });
        }

        [HttpDelete("{id}/atributos/{atributoid}")]
        public  async Task<IActionResult> DeleteAtributo(string id, Guid atributoid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            if (await _repo.BuscaAtributoAsync(atributoid) == null) return NotFound();

            await _repo.RemoverAtributoComDescendentesAsync(atributoid);
            return Ok();
        }

        // ── Opções ───────────────────────────────────────────────────────────────

        public class OpcaoDto
        {
            public string? Nome { get; set; }
            public string? Descricao { get; set; }
            public int Qtd { get; set; }
            public int? Estoque { get; set; }
            public decimal? ValorAdicional { get; set; }
        }

        [HttpPost("{id}/atributos/{atributoid}/opcoes")]
        public  async Task<IActionResult> PostOpcao(string id, Guid atributoid, [FromBody] OpcaoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var o = new Opcao
            {
                Opcaoid        = Guid.NewGuid().ToString(),
                Atributoid     = atributoid,
                Nome           = dto.Nome,
                Descricao      = dto.Descricao,
                Qtd            = dto.Qtd,
                Estoque        = dto.Estoque,
                ValorAdicional = dto.ValorAdicional
            };
            await _repo.CriarOpcaoAsync(o);
            return Ok(o);
        }

        [HttpPut("{id}/atributos/{atributoid}/opcoes/{opcaoid}")]
        public  async Task<IActionResult> PutOpcao(string id, Guid atributoid, string opcaoid, [FromBody] OpcaoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var o = await _repo.BuscaOpcaoAsync(opcaoid, atributoid);
            if (o == null) return NotFound();

            o.Nome           = dto.Nome;
            o.Descricao      = dto.Descricao;
            o.Qtd            = dto.Qtd;
            o.Estoque        = dto.Estoque;
            o.ValorAdicional = dto.ValorAdicional;
            await _repo.AtualizarOpcaoAsync(o);
            return Ok(o);
        }

        [HttpDelete("{id}/atributos/{atributoid}/opcoes/{opcaoid}")]
        public  async Task<IActionResult> DeleteOpcao(string id, Guid atributoid, string opcaoid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var o = await _repo.BuscaOpcaoAsync(opcaoid, atributoid);
            if (o == null) return NotFound();
            await _repo.RemoverOpcaoAsync(o);
            return Ok();
        }

        // ── Galeria de Imagens ────────────────────────────────────────────────────

        [HttpGet("{id}/imagens")]
        public  async Task<IActionResult> GetImagens(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            return Ok(await _repo.ListaImagensPorProdutoAsync(id));
        }

        public class ImagemDto { public string? Url { get; set; } public string? Descricao { get; set; } }

        [HttpPost("{id}/imagens")]
        public  async Task<IActionResult> PostImagem(string id, [FromBody] ImagemDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var img = new Imagem
            {
                Imagemid  = Guid.NewGuid().ToString(),
                Parentid  = id,
                Tipoid    = "produto",
                Url       = dto.Url,
                Descricao = dto.Descricao
            };
            await _repo.CriarImagemAsync(img);
            return Ok(img);
        }

        [HttpDelete("{id}/imagens/{imagemid}")]
        public  async Task<IActionResult> DeleteImagem(string id, string imagemid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var img = await _repo.BuscaImagemAsync(imagemid, id);
            if (img == null) return NotFound();
            await _repo.RemoverImagemAsync(img);
            return Ok();
        }

        // ── Geração de descrição via IA (visão) ──────────────────────────────────

        [HttpPost("gerar-descricao")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GerarDescricao(
            [FromForm] IFormFile? arquivo,
            [FromForm] string? imageUrl,
            [FromForm] string? provedor,
            [FromForm] string? produtoid)
        {
            var (acessoTotal, claimAppId) = UserContext();

            byte[] imageBytes;
            string mimeType;
            string? imagemSalvaUrl = null;

            if (arquivo != null && arquivo.Length > 0)
            {
                using var ms = new MemoryStream();
                await arquivo.CopyToAsync(ms);
                imageBytes = ms.ToArray();
                mimeType = arquivo.ContentType ?? "image/jpeg";

                var appFolder = claimAppId ?? "geral";
                var uploadsPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", appFolder);
                Directory.CreateDirectory(uploadsPath);
                var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{ext}";
                await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadsPath, fileName), imageBytes);
                imagemSalvaUrl = $"/uploads/{appFolder}/{fileName}";

                if (!string.IsNullOrEmpty(produtoid))
                {
                    var produto = await _repo.BuscaPorIdAsync(produtoid);
                    if (produto != null && (acessoTotal || produto.Aplicacaoid == claimAppId))
                    {
                        await _repo.CriarImagemAsync(new Imagem
                        {
                            Imagemid  = Guid.NewGuid().ToString(),
                            Parentid  = produtoid,
                            Tipoid    = "produto",
                            Url       = imagemSalvaUrl,
                            Descricao = "Gerado por IA"
                        });
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                using var http = new HttpClient();
                imageBytes = await http.GetByteArrayAsync(imageUrl);
                var ext = Path.GetExtension(imageUrl).ToLowerInvariant().TrimStart('.');
                mimeType = ext switch
                {
                    "jpg" or "jpeg" => "image/jpeg",
                    "png"           => "image/png",
                    "webp"          => "image/webp",
                    "gif"           => "image/gif",
                    _               => "image/jpeg"
                };
                imagemSalvaUrl = imageUrl;
            }
            else
            {
                return BadRequest("Informe uma URL ou faça upload de uma imagem.");
            }

            var prompt = @"Analise esta imagem de produto e retorne APENAS um JSON válido, sem texto adicional:
{
  ""nome"": ""<nome do produto>"",
  ""descricacurta"": ""<descrição curta, máx 120 caracteres>"",
  ""descricao"": ""<descrição completa do produto>"",
  ""detalhetecnico"": ""<especificações técnicas, materiais, dimensões se visíveis>"",
  ""atributos"": [
    { ""nome"": ""<nome do atributo, ex: Cor, Tamanho>"", ""opcoes"": [{ ""nome"": ""<valor>"", ""estoque"": 0 }] }
  ]
}
Se não identificar variações ou atributos na imagem, retorne ""atributos"" como array vazio [].
Responda em português do Brasil.";

            try
            {
                var agente = _agentFactory.Criar(provedor);
                var raw = LimparMarkdown(await agente.GerarComImagemAsync(imageBytes, mimeType, prompt));
                var dados = JsonDocument.Parse(raw).RootElement;
                var atributos = dados.TryGetProperty("atributos", out var atrib) ? atrib : (JsonElement?)null;
                return Ok(new
                {
                    nome           = dados.GetProperty("nome").GetString(),
                    descricacurta  = dados.GetProperty("descricacurta").GetString(),
                    descricao      = dados.GetProperty("descricao").GetString(),
                    detalhetecnico = dados.GetProperty("detalhetecnico").GetString(),
                    imagemUrl      = imagemSalvaUrl,
                    atributos      = atributos?.ValueKind == JsonValueKind.Array ? atributos : null,
                    provedor       = agente.Provedor
                });
            }
            catch (JsonException ex)
            {
                return UnprocessableEntity(new { erro = "JSON inválido.", detalhe = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Erro ao chamar o agente IA: {ex.Message}");
            }
        }

        // ── Templates de Produto ─────────────────────────────────────────────────

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var (_, appId) = UserContext();
            if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
            var segmentoIds = await _segmentoRepo.ListaSegmentoIdsPorAplicacaoAsync(appId);
            return Ok(await _templateRepo.ListaComSegmentosAsync(appId, segmentoIds));
        }

        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplateById(string id)
        {
            var (_, appId) = UserContext();
            if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
            var t = await _templateRepo.BuscaPorIdAsync(id, appId);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpPost("templates/interpretar")]
        public async Task<IActionResult> InterpretarTemplate([FromBody] ProdutoTemplateGerarInput input)
        {
            var (_, appId) = UserContext();
            if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");

            var agente = _agentFactory.Criar();
            var raw = await agente.GerarAsync(MontarPromptTemplate(input.Descricao));
            var json = LimparMarkdown(raw);
            try { JsonDocument.Parse(json); }
            catch { return UnprocessableEntity("IA retornou resposta em formato inválido."); }
            return Ok(new { conteudoJson = json });
        }

        [HttpPost("templates")]
        public async Task<IActionResult> PostTemplate([FromBody] ProdutoTemplateInput input)
        {
            var (_, appId) = UserContext();
            if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
            var id = await _templateRepo.CriarAsync(input, appId);
            return Ok(new { produtoTemplateid = id });
        }

        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeleteTemplate(string id)
        {
            var (_, appId) = UserContext();
            if (string.IsNullOrEmpty(appId)) return BadRequest("Token sem aplicacaoid.");
            return await _templateRepo.RemoverAsync(id, appId) ? NoContent() : NotFound();
        }

        [HttpPost("{id}/aplicar-template/{templateId}")]
        public async Task<IActionResult> AplicarTemplate(string id, string templateId)
        {
            var (acessoTotal, appId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != appId) return Forbid();

            var segmentoIds = await _segmentoRepo.ListaSegmentoIdsPorAplicacaoAsync(appId!);
            var template = await _templateRepo.BuscaAcessivelAsync(templateId, appId!, segmentoIds);
            if (template == null) return NotFound();

            var aplicado = await _templateRepo.AplicarTemplateAsync(id, template.ConteudoJson);
            return aplicado ? Ok() : UnprocessableEntity("Template sem atributos para aplicar.");
        }

        private static string MontarPromptTemplate(string descricao) => $$"""
            Você é um especialista em catálogo de produtos. Com base na descrição abaixo, gere um template de produto em JSON seguindo EXATAMENTE a estrutura especificada. Retorne APENAS o JSON válido, sem markdown, sem blocos de código, sem nenhum texto adicional.

            DESCRIÇÃO DO PRODUTO:
            {{descricao}}

            ESTRUTURA OBRIGATÓRIA DO JSON:
            {
              "nome": "nome do produto",
              "descricao": "descrição completa",
              "descricaoCurta": "resumo em até 100 caracteres",
              "detalheTecnico": "especificações técnicas relevantes",
              "valor": 0.00,
              "unidadeVenda": "UN",
              "atributos": [
                {
                  "nome": "nome do atributo",
                  "descricao": "descrição",
                  "ordem": 1,
                  "valorAdicional": 0.00,
                  "opcoes": ["opção 1", "opção 2"],
                  "filhos": []
                }
              ]
            }

            REGRAS:
            - Gere atributos e opções relevantes para o tipo de produto descrito
            - Atributos hierárquicos (componentes de um produto composto) usam a propriedade "filhos" com a mesma estrutura
            - opcoes é um array de strings com os valores possíveis
            - Valores monetários são decimais (ex: 10.50); use 0.00 quando desconhecido
            - unidadeVenda deve ser abreviação padrão: UN, KG, M, M2, M3, CX, PCT, L, etc.
            - Retorne APENAS o JSON, absolutamente nada mais
            """;

        private static string LimparMarkdown(string texto)
        {
            var inicio = texto.IndexOf('{');
            var fim = texto.LastIndexOf('}');
            if (inicio >= 0 && fim > inicio)
                return texto[inicio..(fim + 1)];
            return texto.Trim();
        }

        // ── Mão de Obra ──────────────────────────────────────────────────────────

        public class MaoDeObraDto
        {
            public string Tipo { get; set; } = "";
            public string Descricao { get; set; } = "";
            public int? CapacidadeDia { get; set; }
            public decimal? ValorDia { get; set; }
            public decimal? ValorMilheiro { get; set; }
        }

        [HttpGet("{id}/maodeobra")]
        public  async Task<IActionResult> GetMaoDeObra(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            return Ok(await _moRepo.ListarPorProdutoAsync(id));
        }

        [HttpPost("{id}/maodeobra")]
        public  async Task<IActionResult> PostMaoDeObra(string id, [FromBody] MaoDeObraDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            if (dto.Tipo != "capacidade_dia" && dto.Tipo != "milheiro")
                return BadRequest("Tipo deve ser 'capacidade_dia' ou 'milheiro'.");

            var mo = new ProdutoMaoDeObra
            {
                Id            = Guid.NewGuid(),
                Produtoid     = id,
                Tipo          = dto.Tipo,
                Descricao     = dto.Descricao,
                CapacidadeDia = dto.CapacidadeDia,
                ValorDia      = dto.ValorDia,
                ValorMilheiro = dto.ValorMilheiro
            };
            return Ok(await _moRepo.CriarAsync(mo));
        }

        [HttpPut("{id}/maodeobra/{moid}")]
        public  async Task<IActionResult> PutMaoDeObra(string id, Guid moid, [FromBody] MaoDeObraDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var mo = await _moRepo.BuscarPorIdAsync(moid);
            if (mo == null || mo.Produtoid != id) return NotFound();

            if (dto.Tipo != "capacidade_dia" && dto.Tipo != "milheiro")
                return BadRequest("Tipo deve ser 'capacidade_dia' ou 'milheiro'.");

            mo.Tipo          = dto.Tipo;
            mo.Descricao     = dto.Descricao;
            mo.CapacidadeDia = dto.CapacidadeDia;
            mo.ValorDia      = dto.ValorDia;
            mo.ValorMilheiro = dto.ValorMilheiro;
            return Ok(await _moRepo.AtualizarAsync(mo));
        }

        [HttpDelete("{id}/maodeobra/{moid}")]
        public  async Task<IActionResult> DeleteMaoDeObra(string id, Guid moid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = await _repo.BuscaPorIdAsync(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var mo = await _moRepo.BuscarPorIdAsync(moid);
            if (mo == null || mo.Produtoid != id) return NotFound();

            await _moRepo.RemoverAsync(mo);
            return Ok();
        }
    }
}
