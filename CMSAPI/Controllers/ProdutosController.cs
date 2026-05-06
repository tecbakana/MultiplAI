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

        public ProdutosController(
            IProdutosRepositorio repo,
            IAgentIAFactory agentFactory,
            IWebHostEnvironment env,
            IProdutoMaoDeObraRepositorio moRepo)
        {
            _repo = repo;
            _agentFactory = agentFactory;
            _env = env;
            _moRepo = moRepo;
        }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        // ── CRUD Produto ─────────────────────────────────────────────────────────

        [HttpGet]
        public IEnumerable<Produto> Get([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var resolvedAppId = acessoTotal
                ? (string.IsNullOrEmpty(aplicacaoid) ? null : aplicacaoid)
                : claimAppId;
            return _repo.Lista(resolvedAppId);
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
        public IActionResult Post([FromBody] NovoProdutoDto dto)
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
            _repo.Criar(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] NovoProdutoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaPorId(id);
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
            _repo.Atualizar(item);
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
            _repo.Remover(item);
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
        public IActionResult GetAtributos(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var arvore = _repo.BuscaArvoreComOpcoes(id);
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
        public IActionResult PostAtributo(string id, [FromBody] AtributoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            if (dto.ParentAtributoId.HasValue && _repo.BuscaAtributo(dto.ParentAtributoId.Value) == null)
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
            _repo.CriarAtributo(a);
            return Ok(new { a.Atributoid, a.Nome, a.Descricao, a.Ordem, a.ParentAtributoId, a.Produtoid });
        }

        [HttpPut("{id}/atributos/{atributoid}")]
        public IActionResult PutAtributo(string id, Guid atributoid, [FromBody] AtributoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var a = _repo.BuscaAtributo(atributoid);
            if (a == null) return NotFound();

            if (dto.ParentAtributoId.HasValue && dto.ParentAtributoId != a.ParentAtributoId &&
                _repo.BuscaAtributo(dto.ParentAtributoId.Value) == null)
                return BadRequest("ParentAtributoId não encontrado.");

            a.Nome             = dto.Nome;
            a.Descricao        = dto.Descricao ?? a.Descricao;
            a.Ordem            = dto.Ordem ?? a.Ordem;
            a.ParentAtributoId = dto.ParentAtributoId;
            a.ValorAdicional   = dto.ValorAdicional;
            _repo.AtualizarAtributo(a);
            return Ok(new { a.Atributoid, a.Nome, a.Descricao, a.Ordem, a.ParentAtributoId, a.Produtoid });
        }

        [HttpDelete("{id}/atributos/{atributoid}")]
        public IActionResult DeleteAtributo(string id, Guid atributoid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            if (_repo.BuscaAtributo(atributoid) == null) return NotFound();

            _repo.RemoverAtributoComDescendentes(atributoid);
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
        public IActionResult PostOpcao(string id, Guid atributoid, [FromBody] OpcaoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
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
            _repo.CriarOpcao(o);
            return Ok(o);
        }

        [HttpPut("{id}/atributos/{atributoid}/opcoes/{opcaoid}")]
        public IActionResult PutOpcao(string id, Guid atributoid, string opcaoid, [FromBody] OpcaoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var o = _repo.BuscaOpcao(opcaoid, atributoid);
            if (o == null) return NotFound();

            o.Nome           = dto.Nome;
            o.Descricao      = dto.Descricao;
            o.Qtd            = dto.Qtd;
            o.Estoque        = dto.Estoque;
            o.ValorAdicional = dto.ValorAdicional;
            _repo.AtualizarOpcao(o);
            return Ok(o);
        }

        [HttpDelete("{id}/atributos/{atributoid}/opcoes/{opcaoid}")]
        public IActionResult DeleteOpcao(string id, Guid atributoid, string opcaoid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var o = _repo.BuscaOpcao(opcaoid, atributoid);
            if (o == null) return NotFound();
            _repo.RemoverOpcao(o);
            return Ok();
        }

        // ── Galeria de Imagens ────────────────────────────────────────────────────

        [HttpGet("{id}/imagens")]
        public IActionResult GetImagens(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            return Ok(_repo.ListaImagensPorProduto(id));
        }

        public class ImagemDto { public string? Url { get; set; } public string? Descricao { get; set; } }

        [HttpPost("{id}/imagens")]
        public IActionResult PostImagem(string id, [FromBody] ImagemDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
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
            _repo.CriarImagem(img);
            return Ok(img);
        }

        [HttpDelete("{id}/imagens/{imagemid}")]
        public IActionResult DeleteImagem(string id, string imagemid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var img = _repo.BuscaImagem(imagemid, id);
            if (img == null) return NotFound();
            _repo.RemoverImagem(img);
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
                    var produto = _repo.BuscaPorId(produtoid);
                    if (produto != null && (acessoTotal || produto.Aplicacaoid == claimAppId))
                    {
                        _repo.CriarImagem(new Imagem
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
        public IActionResult GetMaoDeObra(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            return Ok(_moRepo.ListarPorProduto(id));
        }

        [HttpPost("{id}/maodeobra")]
        public IActionResult PostMaoDeObra(string id, [FromBody] MaoDeObraDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
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
            return Ok(_moRepo.Criar(mo));
        }

        [HttpPut("{id}/maodeobra/{moid}")]
        public IActionResult PutMaoDeObra(string id, Guid moid, [FromBody] MaoDeObraDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var mo = _moRepo.BuscarPorId(moid);
            if (mo == null || mo.Produtoid != id) return NotFound();

            if (dto.Tipo != "capacidade_dia" && dto.Tipo != "milheiro")
                return BadRequest("Tipo deve ser 'capacidade_dia' ou 'milheiro'.");

            mo.Tipo          = dto.Tipo;
            mo.Descricao     = dto.Descricao;
            mo.CapacidadeDia = dto.CapacidadeDia;
            mo.ValorDia      = dto.ValorDia;
            mo.ValorMilheiro = dto.ValorMilheiro;
            return Ok(_moRepo.Atualizar(mo));
        }

        [HttpDelete("{id}/maodeobra/{moid}")]
        public IActionResult DeleteMaoDeObra(string id, Guid moid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _repo.BuscaPorId(id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var mo = _moRepo.BuscarPorId(moid);
            if (mo == null || mo.Produtoid != id) return NotFound();

            _moRepo.Remover(mo);
            return Ok();
        }
    }
}
