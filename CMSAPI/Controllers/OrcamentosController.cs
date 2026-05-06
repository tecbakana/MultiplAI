using ICMSX;
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

    public OrcamentosController(
        IOrcamentoRepositorio orcamentoRepo,
        IOrcamentoCompostoRepositorio compostoRepo)
    {
        _orcamentoRepo = orcamentoRepo;
        _compostoRepo = compostoRepo;
    }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True",
         User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public  async Task<IActionResult> Get([FromQuery] string? aplicacaoid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = acessoTotal && !string.IsNullOrEmpty(aplicacaoid) ? aplicacaoid : claimAppId;

        var lista = (await _orcamentoRepo.ListaAsync(appId!))
            .Select(o => new
            {
                o.Orcamentoid,
                o.Nome,
                o.Email,
                o.Telefone,
                o.Nomevendedor,
                o.Valorestimado,
                o.Prazo,
                o.Aprovado,
                o.Datainclusao
            });

        return Ok(lista);
    }

    [HttpGet("{id}")]
    public  async Task<IActionResult> GetById(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();

        var orcamento = await _orcamentoRepo.BuscaPorIdAsync(id);
        if (orcamento == null) return NotFound();
        if (!acessoTotal && orcamento.Aplicacaoid != claimAppId) return Forbid();

        var itensCompostos = (await _compostoRepo.ListarAtuaisAsync(orcamento.Orcamentoid))
            .Select(d => new
            {
                d.OrcamentoDetalheCompostoId,
                d.Produtoid,
                d.Quantidade,
                d.ValorBase,
                d.ValorTotal,
                ConfiguracaoJson = d.ConfiguracaoJson,
                d.Versao
            })
            .ToList();

        return Ok(new
        {
            orcamento.Orcamentoid,
            orcamento.Aplicacaoid,
            orcamento.Nome,
            orcamento.Email,
            orcamento.Telefone,
            orcamento.Descricaoservico,
            orcamento.Valorestimado,
            orcamento.Prazo,
            orcamento.Nomevendedor,
            orcamento.Aprovado,
            orcamento.Datainclusao,
            Itens = orcamento.OrcamentoDetalhes.Select(d => new
            {
                d.Orcamentodetalheid,
                d.Descricao,
                d.Quantidade,
                d.Valor,
                d.Ativo
            }).ToList(),
            ItensCompostos = itensCompostos
        });
    }

    [HttpPut("{id}/aprovar")]
    public  async Task<IActionResult> Aprovar(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var orcamento = await _orcamentoRepo.BuscaPorIdAsync(id);
        if (orcamento == null) return NotFound();
        if (!acessoTotal && orcamento.Aplicacaoid != claimAppId) return Forbid();

        await _orcamentoRepo.ToggleAprovadoAsync(orcamento);
        return Ok(new { orcamento.Aprovado });
    }

    [HttpDelete("{id}")]
    public  async Task<IActionResult> Delete(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var orcamento = await _orcamentoRepo.BuscaPorIdAsync(id);
        if (orcamento == null) return NotFound();
        if (!acessoTotal && orcamento.Aplicacaoid != claimAppId) return Forbid();

        await _compostoRepo.RemoverPorOrcamentoAsync(orcamento.Orcamentoid);
        await _orcamentoRepo.RemoveAsync(orcamento);
        return Ok();
    }
}
