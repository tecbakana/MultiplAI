using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FaqController : Controller
    {
        private readonly IFaqRepositorio _repo;
        public FaqController(IFaqRepositorio repo) { _repo = repo; }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        // ── Wiki pública ─────────────────────────────────────────────────────

        [AllowAnonymous]
        [HttpGet("wiki/{aplicacaoid}")]
        public  async Task<IActionResult> GetWiki(string aplicacaoid)
        {
            var categorias = (await _repo.ListaCategoriasPorAppAsync(aplicacaoid))
                .Select(c => (Cateriaid: c.Cateriaid!, c.Nome, c.Cateriaidpai))
                .ToList();

            var formularios = (await _repo.ListaFormulariosComCategoriaAsync(aplicacaoid))
                .Select(f => (Formularioid: f.Formularioid!, f.Nome, f.Categoriaid))
                .ToList();

            var formularioIds = formularios.Select(f => f.Formularioid).ToList();

            var faqs = (await _repo.ListaFaqsAtivosAsync(formularioIds))
                .Select(faq => new FaqWiki(faq.Faqid, faq.Formularioid, faq.Pergunta, faq.Resposta, faq.Ordem))
                .ToList();

            var arvore = categorias
                .Where(c => c.Cateriaidpai == null)
                .Select(c => MontarNo(c.Cateriaid, c.Nome, categorias, formularios, faqs))
                .ToList();

            return Ok(arvore);
        }

        private record CategoriaWiki(string Cateriaid, string? Nome, List<CategoriaWiki> Subcategorias, List<FormularioWiki> Formularios);
        private record FormularioWiki(string Formularioid, string? Nome, List<FaqWiki> Items);
        private record FaqWiki(string Faqid, string Formularioid, string Pergunta, string Resposta, int Ordem);

        private CategoriaWiki MontarNo(
            string cateriaid, string? nome,
            IReadOnlyList<(string Cateriaid, string? Nome, string? Cateriaidpai)> categorias,
            IReadOnlyList<(string Formularioid, string? Nome, string? Categoriaid)> formularios,
            IReadOnlyList<FaqWiki> faqs)
        {
            var subcats = categorias
                .Where(c => c.Cateriaidpai == cateriaid)
                .Select(c => MontarNo(c.Cateriaid, c.Nome, categorias, formularios, faqs))
                .ToList();

            var forms = formularios
                .Where(f => f.Categoriaid == cateriaid)
                .Select(f => new FormularioWiki(
                    f.Formularioid, f.Nome,
                    faqs.Where(faq => faq.Formularioid == f.Formularioid).ToList()))
                .ToList();

            return new CategoriaWiki(cateriaid, nome, subcats, forms);
        }

        // ── CRUD autenticado ─────────────────────────────────────────────────

        [Authorize]
        [HttpGet("{formularioid}")]
        public  async Task<IActionResult> GetByFormulario(string formularioid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            if (!acessoTotal && !await _repo.TemAcessoAsync(formularioid, claimAppId)) return Forbid();

            return Ok(await _repo.ListaPorFormularioAsync(formularioid));
        }

        public class FaqDto
        {
            public string? Pergunta { get; set; }
            public string? Resposta { get; set; }
            public int Ordem { get; set; }
        }

        [Authorize]
        [HttpPost("{formularioid}")]
        public  async Task<IActionResult> Post(string formularioid, [FromBody] FaqDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            if (!acessoTotal && !await _repo.TemAcessoAsync(formularioid, claimAppId)) return Forbid();

            var item = new Faq
            {
                Faqid        = Guid.NewGuid().ToString(),
                Formularioid = formularioid,
                Pergunta     = dto.Pergunta ?? "",
                Resposta     = dto.Resposta ?? "",
                Ordem        = dto.Ordem,
                Ativo        = true,
                Datainclusao = DateTime.UtcNow
            };
            await _repo.CriarAsync(item);
            return Ok(item);
        }

        [Authorize]
        [HttpPut("{id}")]
        public  async Task<IActionResult> Put(string id, [FromBody] FaqDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal && !await _repo.TemAcessoAsync(item.Formularioid, claimAppId)) return Forbid();

            item.Pergunta = dto.Pergunta ?? item.Pergunta;
            item.Resposta = dto.Resposta ?? item.Resposta;
            item.Ordem    = dto.Ordem;
            await _repo.AtualizarAsync(item);
            return Ok(item);
        }

        [Authorize]
        [HttpPatch("{id}/ativo")]
        public  async Task<IActionResult> PatchAtivo(string id, [FromBody] bool ativo)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal && !await _repo.TemAcessoAsync(item.Formularioid, claimAppId)) return Forbid();

            item.Ativo = ativo;
            await _repo.AtualizarAsync(item);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public  async Task<IActionResult> Delete(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal && !await _repo.TemAcessoAsync(item.Formularioid, claimAppId)) return Forbid();

            await _repo.RemoverAsync(item);
            return Ok();
        }

        // ── Promoção de resposta do inbox para FAQ ───────────────────────────

        [Authorize]
        [HttpPost("promover/{idform}")]
        public  async Task<IActionResult> Promover(int idform, [FromBody] FaqDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var resposta = await _repo.BuscaRespostaInboxAsync(idform);
            if (resposta == null) return NotFound();
            if (!acessoTotal && !await _repo.TemAcessoAsync(resposta.Formularioid, claimAppId)) return Forbid();

            var item = new Faq
            {
                Faqid        = Guid.NewGuid().ToString(),
                Formularioid = resposta.Formularioid!,
                Pergunta     = dto.Pergunta ?? "",
                Resposta     = dto.Resposta ?? resposta.Texto ?? "",
                Ordem        = dto.Ordem,
                Ativo        = true,
                Datainclusao = DateTime.UtcNow
            };
            await _repo.CriarAsync(item);
            return Ok(item);
        }
    }
}
