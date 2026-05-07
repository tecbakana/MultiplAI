using CMSXData.Models;
using ICMSX;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CMSAPIPublica.Controllers;

[ApiController]
[Route("api/publico/orcamento")]
[EnableRateLimiting("api_publica")]
public class OrcamentoPublicoController : ControllerBase
{
    private const int MaxDescricaoLen = 2000;

    private readonly CmsxDbContext _context;
    private readonly IAtributoRepositorio _atributoRepo;
    private readonly IOrcamentoCompostoRepositorio _compostoRepo;
    private readonly IModeloCompostoRepositorio _modeloRepo;

    public OrcamentoPublicoController(
        CmsxDbContext context,
        IAtributoRepositorio atributoRepo,
        IOrcamentoCompostoRepositorio compostoRepo,
        IModeloCompostoRepositorio modeloRepo)
    {
        _context = context;
        _atributoRepo = atributoRepo;
        _compostoRepo = compostoRepo;
        _modeloRepo = modeloRepo;
    }

    private string? ResolverAplicacaoid(string token)
    {
        var registro = _context.PublicTokens.FirstOrDefault(t =>
            t.Token == token &&
            t.Ativo &&
            (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow));

        return registro?.Aplicacaoid;
    }

    [HttpGet("produtos")]
    public async Task<IActionResult> GetProdutos([FromQuery] string @ref)
    {
        if (string.IsNullOrWhiteSpace(@ref)) return BadRequest("token obrigatório");

        var aplicacaoid = ResolverAplicacaoid(@ref);
        if (aplicacaoid == null) return NotFound("token inválido ou expirado");

        var produtos = _context.Produtos
            .Where(p => p.Aplicacaoid == aplicacaoid)
            .OrderBy(p => p.Nome)
            .Select(p => new { p.Produtoid, p.Nome, p.Valor, p.UnidadeVenda })
            .ToList();

        var produtoIds = produtos.Select(p => p.Produtoid).ToList();

        var todosAtributos = await _atributoRepo.ListaAtributosArvoreAsync(produtoIds);
        var todosAtributoIds = todosAtributos.Select(a => a.Atributoid).ToList();

        var opcoes = _context.Opcaos
            .Where(o => todosAtributoIds.Contains(o.Atributoid))
            .Select(o => new { o.Opcaoid, o.Nome, o.Atributoid, o.ValorAdicional })
            .ToList();

        var todosMos = _context.ProdutoMaoDeObras
            .Where(m => produtoIds.Contains(m.Produtoid))
            .ToList();

        object BuildArvore(Atributo a)
        {
            var filhos = todosAtributos
                .Where(f => f.ParentAtributoId == a.Atributoid)
                .OrderBy(f => f.Ordem)
                .Select(f => BuildArvore(f))
                .ToList();

            var opcoesNo = opcoes
                .Where(o => o.Atributoid == a.Atributoid)
                .Select(o => new { opcaoid = o.Opcaoid, nome = o.Nome, valorAdicional = o.ValorAdicional ?? 0m })
                .ToList<object>();

            return new
            {
                atributoid     = a.Atributoid.ToString(),
                nome           = a.Nome,
                ordem          = a.Ordem,
                valorAdicional = a.ValorAdicional ?? 0m,
                opcoes         = opcoesNo,
                filhos
            };
        }

        var result = produtos.Select(p => new
        {
            produtoid    = p.Produtoid,
            nome         = p.Nome,
            valor        = p.Valor,
            unidadeVenda = p.UnidadeVenda,
            atributos    = todosAtributos
                .Where(a => a.Produtoid == p.Produtoid)
                .OrderBy(a => a.Ordem)
                .Select(a => BuildArvore(a))
                .ToList(),
            maosDeObra   = todosMos
                .Where(m => m.Produtoid == p.Produtoid)
                .Select(m => new
                {
                    id            = m.Id,
                    tipo          = m.Tipo,
                    descricao     = m.Descricao,
                    capacidadeDia = m.CapacidadeDia,
                    valorDia      = m.ValorDia,
                    valorMilheiro = m.ValorMilheiro
                }).ToList()
        }).ToList();

        return Ok(result);
    }

    [HttpGet("modelos")]
    public async Task<IActionResult> GetModelos([FromQuery] string produto, [FromQuery] string @ref)
    {
        if (string.IsNullOrWhiteSpace(@ref)) return BadRequest("token obrigatório");
        if (string.IsNullOrWhiteSpace(produto)) return BadRequest("produto obrigatório");

        var aplicacaoid = ResolverAplicacaoid(@ref);
        if (aplicacaoid == null) return NotFound("token inválido ou expirado");

        var modelos = (await _modeloRepo.ListarPorProdutoAsync(aplicacaoid, produto))
            .Select(m => new
            {
                m.ModeloCompostoId,
                m.Nome,
                m.ValorUnitario,
                m.Usos
            });

        return Ok(modelos);
    }

    public class SelecaoCompostoDto
    {
        public Guid Atributoid { get; set; }
        public string Opcaoid { get; set; } = "";
    }

    public class ItemCompostoDto
    {
        public string Produtoid { get; set; } = "";
        public decimal Quantidade { get; set; } = 1;
        public List<SelecaoCompostoDto> Selecoes { get; set; } = new();
        public List<Guid> CheckboxAtributos { get; set; } = new();
    }

    public class ItemOrcamentoDto
    {
        public string Descricao { get; set; } = "";
        public decimal Quantidade { get; set; } = 1;
        public decimal? Valor { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class NovoOrcamentoPublicoDto
    {
        public string Token { get; set; } = "";
        public string Nome { get; set; } = "";
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Descricaoservico { get; set; }
        public decimal? Valorestimado { get; set; }
        public string? Prazo { get; set; }
        public string? Nomevendedor { get; set; }
        public List<ItemOrcamentoDto> Itens { get; set; } = new();
        public List<ItemCompostoDto> ItensCompostos { get; set; } = new();
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] NovoOrcamentoPublicoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest("token obrigatório");

        if (string.IsNullOrWhiteSpace(dto.Nome))
            return BadRequest("nome obrigatório");

        var aplicacaoid = ResolverAplicacaoid(dto.Token);
        if (aplicacaoid == null) return NotFound("token inválido ou expirado");

        var descricao = dto.Descricaoservico?.Trim();
        if (descricao?.Length > MaxDescricaoLen)
            descricao = descricao[..MaxDescricaoLen];

        var orcamento = new OrcamentoCabecalho
        {
            Orcamentoid      = Guid.NewGuid(),
            Aplicacaoid      = aplicacaoid,
            Nome             = dto.Nome.Trim(),
            Email            = dto.Email?.Trim(),
            Telefone         = dto.Telefone?.Trim(),
            Descricaoservico = descricao,
            Valorestimado    = dto.Valorestimado,
            Prazo            = dto.Prazo?.Trim(),
            Nomevendedor     = dto.Nomevendedor?.Trim(),
            Aprovado         = false,
            Datainclusao     = DateTime.UtcNow
        };

        _context.OrcamentoCabecalhos.Add(orcamento);

        foreach (var item in dto.Itens.Where(i => !string.IsNullOrWhiteSpace(i.Descricao)))
        {
            _context.OrcamentoDetalhes.Add(new OrcamentoDetalhe
            {
                Orcamentodetalheid = Guid.NewGuid(),
                Orcamentoid        = orcamento.Orcamentoid,
                Descricao          = item.Descricao.Trim(),
                Quantidade         = item.Quantidade,
                Valor              = item.Valor,
                Ativo              = item.Ativo
            });
        }

        await _context.SaveChangesAsync();

        // Processar itens compostos
        foreach (var itemComposto in dto.ItensCompostos.Where(i => !string.IsNullOrWhiteSpace(i.Produtoid) && i.Selecoes.Count > 0))
        {
            var produto = (await _compostoRepo.BuscarProdutoAsync(itemComposto.Produtoid));
            if (produto == null) continue;

            var opcaoIds = itemComposto.Selecoes.Select(s => s.Opcaoid).ToList();
            var opcoes = (await _compostoRepo.BuscarOpcoesAsync(opcaoIds)).ToDictionary(o => o.Opcaoid);

            var atributoIds = itemComposto.Selecoes.Select(s => s.Atributoid).ToList();
            var atributos = (await _compostoRepo.BuscarAtributosAsync(atributoIds)).ToDictionary(a => a.Atributoid);

            // Calcular valor server-side — nunca confiar no cliente
            var valorBase = produto.Valor ?? 0m;
            var somaAdicionais = itemComposto.Selecoes
                .Where(s => opcoes.ContainsKey(s.Opcaoid))
                .Sum(s => opcoes[s.Opcaoid].ValorAdicional ?? 0m);

            var checkboxAtributoIds = itemComposto.CheckboxAtributos ?? new();
            var checkboxAtributosMap = checkboxAtributoIds.Count > 0
                ? (await _compostoRepo.BuscarAtributosAsync(checkboxAtributoIds)).ToDictionary(a => a.Atributoid)
                : new Dictionary<Guid, Atributo>();
            var somaCheckboxes = checkboxAtributosMap.Values.Sum(a => a.ValorAdicional ?? 0m);

            var valorUnitario = valorBase + somaAdicionais + somaCheckboxes;
            var valorProdutos = valorUnitario * itemComposto.Quantidade;

            var mos = _context.ProdutoMaoDeObras
                .Where(m => m.Produtoid == itemComposto.Produtoid)
                .ToList();
            var mosCusto = mos.Select(m => new
            {
                descricao = m.Descricao,
                tipo      = m.Tipo,
                unidades  = CalcularUnidadesMo(m, itemComposto.Quantidade),
                custo     = CalcularCustoMo(m, itemComposto.Quantidade)
            }).ToList();
            var totalMo = mosCusto.Sum(m => m.custo);

            var valorTotal = valorProdutos + totalMo;

            // Carregar ancestrais para montar o caminho no JSON de snapshot
            var todosAncestralIds = new HashSet<Guid>(atributoIds);
            var fila = new Queue<Guid>(atributoIds);
            foreach (var cbId in checkboxAtributoIds)
                if (todosAncestralIds.Add(cbId)) fila.Enqueue(cbId);
            while (fila.Count > 0)
            {
                var atualId = fila.Dequeue();
                if (atributos.TryGetValue(atualId, out var atr) && atr.ParentAtributoId.HasValue)
                {
                    if (todosAncestralIds.Add(atr.ParentAtributoId.Value))
                        fila.Enqueue(atr.ParentAtributoId.Value);
                }
            }

            var todosAtributos = (await _compostoRepo.BuscarAtributosAsync(todosAncestralIds))
                .ToDictionary(a => a.Atributoid);
            foreach (var (k, v) in atributos) todosAtributos.TryAdd(k, v);

            string BuildCaminho(Guid atributoId)
            {
                var partes = new List<string>();
                var atual = todosAtributos.GetValueOrDefault(atributoId);
                while (atual != null)
                {
                    partes.Insert(0, atual.Nome);
                    atual = atual.ParentAtributoId.HasValue
                        ? todosAtributos.GetValueOrDefault(atual.ParentAtributoId.Value)
                        : null;
                }
                return string.Join(" > ", partes);
            }

            var selecoesCfg = itemComposto.Selecoes
                .Select(s => new
                {
                    atributo  = BuildCaminho(s.Atributoid),
                    opcao     = opcoes.TryGetValue(s.Opcaoid, out var op) ? op.Nome ?? s.Opcaoid : s.Opcaoid,
                    adicional = op?.ValorAdicional ?? 0m
                })
                .ToList();

            var checkboxesCfg = checkboxAtributosMap.Values
                .Select(a => new { atributo = BuildCaminho(a.Atributoid), adicional = a.ValorAdicional ?? 0m })
                .ToList();

            var configuracaoJson = JsonSerializer.Serialize(new
            {
                produto      = produto.Nome,
                valorBase,
                quantidade   = itemComposto.Quantidade,
                selecoes     = selecoesCfg,
                checkboxes   = checkboxesCfg,
                valorUnitario,
                maosDeObra   = mosCusto,
                totalMo,
                valorTotal
            });

            var detalhe = new OrcamentoDetalheComposto
            {
                OrcamentoDetalheCompostoId = Guid.NewGuid(),
                Orcamentoid   = orcamento.Orcamentoid,
                Produtoid     = itemComposto.Produtoid,
                Quantidade    = itemComposto.Quantidade,
                ValorBase     = valorBase,
                ValorTotal    = valorTotal,
                ConfiguracaoJson = configuracaoJson,
                Versao        = 1,
                Atual         = true,
                Datainclusao  = DateTime.UtcNow
            };

            var selecoes = itemComposto.Selecoes
                .Where(s => opcoes.ContainsKey(s.Opcaoid))
                .Select(s => new Selecao
                {
                    SelecaoId                  = Guid.NewGuid(),
                    OrcamentoDetalheCompostoId = detalhe.OrcamentoDetalheCompostoId,
                    Atributoid                 = s.Atributoid,
                    Opcaoid                    = s.Opcaoid,
                    ValorAdicional             = opcoes[s.Opcaoid].ValorAdicional ?? 0m
                })
                .ToList();

            await _compostoRepo.CriarAsync(detalhe, selecoes);

            // Deduplica ModeloComposto pelo hash das opções + checkboxes
            var hashEntries = opcaoIds.Concat(checkboxAtributoIds.Select(id => "cb:" + id.ToString("N")));
            var hash = ComputarHashOpcoes(hashEntries);
            var nomeModelo = string.Join(" / ",
                selecoesCfg.Select(s => s.opcao)
                    .Concat(checkboxesCfg.Select(c => c.atributo)));

            var modelo = new ModeloComposto
            {
                ModeloCompostoId = Guid.NewGuid(),
                Aplicacaoid      = aplicacaoid,
                Produtoid        = itemComposto.Produtoid,
                Nome             = nomeModelo,
                ValorUnitario    = valorUnitario,
                ConfiguracaoHash = hash,
                Usos             = 1,
                Datacriacao      = DateTime.UtcNow
            };

            var modeloSelecoes = itemComposto.Selecoes.Select(s => new ModeloSelecao
            {
                ModeloSelecaoId  = Guid.NewGuid(),
                ModeloCompostoId = modelo.ModeloCompostoId,
                Atributoid       = s.Atributoid,
                Opcaoid          = s.Opcaoid
            }).ToList();

            await _modeloRepo.CriarOuIncrementarAsync(modelo, modeloSelecoes);
        }

        return Ok(new { orcamento.Orcamentoid });
    }

    private static int CalcularUnidadesMo(ProdutoMaoDeObra mo, decimal quantidade)
    {
        if (mo.Tipo == "capacidade_dia" && mo.CapacidadeDia.HasValue)
            return (int)Math.Ceiling((double)quantidade / mo.CapacidadeDia.Value);
        if (mo.Tipo == "milheiro")
            return (int)Math.Ceiling((double)quantidade / 1000);
        return 1;
    }

    private static decimal CalcularCustoMo(ProdutoMaoDeObra mo, decimal quantidade)
    {
        if (mo.Tipo == "capacidade_dia" && mo.CapacidadeDia.HasValue && mo.ValorDia.HasValue)
        {
            var dias = (int)Math.Ceiling((double)quantidade / mo.CapacidadeDia.Value);
            return dias * mo.ValorDia.Value;
        }
        if (mo.Tipo == "milheiro" && mo.ValorMilheiro.HasValue)
        {
            var milheiros = (int)Math.Ceiling((double)quantidade / 1000);
            return milheiros * mo.ValorMilheiro.Value;
        }
        return 0m;
    }

    private static string ComputarHashOpcoes(IEnumerable<string> opcaoIds)
    {
        var input = string.Join(",", opcaoIds.OrderBy(x => x));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
