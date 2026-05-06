using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class FormulariosController : Controller
    {
        private readonly IFormularioRepositorio _repo;
        public FormulariosController(IFormularioRepositorio repo) { _repo = repo; }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        // ── Definições de formulário ─────────────────────────────────────────

        [HttpGet("defs")]
        public async Task<IEnumerable<object>> GetDefs([FromQuery] string? areaid = null, [FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var filtroApp = acessoTotal ? aplicacaoid : claimAppId;
            return (await _repo.ListaDefsAsync(filtroApp, areaid))
                .Select(f => new { f.Formularioid, f.Nome, f.Valor, f.Ativo, f.Datainclusao, f.Areaid, f.Categoriaid });
        }

        public class FormularioDefDto
        {
            public string? Nome { get; set; }
            public string? Valor { get; set; }
            public string? Areaid { get; set; }
            public string? Categoriaid { get; set; }
        }

        [HttpPost("defs")]
        public  async Task<IActionResult> PostDef([FromBody] FormularioDefDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            if (!acessoTotal && !string.IsNullOrEmpty(dto.Areaid))
            {
                var appId = await _repo.AplicacaoidDaAreaAsync(dto.Areaid);
                if (appId != claimAppId) return Forbid();
            }

            var item = new Formulario
            {
                Formularioid = Guid.NewGuid().ToString(),
                Nome         = dto.Nome ?? "",
                Valor        = dto.Valor,
                Areaid       = dto.Areaid,
                Categoriaid  = dto.Categoriaid,
                Ativo        = true,
                Datainclusao = DateTime.UtcNow
            };
            await _repo.CriarDefAsync(item);
            return Ok(item);
        }

        [HttpPut("defs/{id}")]
        public  async Task<IActionResult> PutDef(string id, [FromBody] FormularioDefDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaDefPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = await _repo.AplicacaoidDaAreaAsync(item.Areaid);
                if (appId != claimAppId) return Forbid();
            }
            item.Nome       = dto.Nome ?? item.Nome;
            item.Valor      = dto.Valor;
            item.Areaid     = dto.Areaid;
            item.Categoriaid = dto.Categoriaid;
            await _repo.AtualizarDefAsync(item);
            return Ok(item);
        }

        [HttpDelete("defs/{id}")]
        public  async Task<IActionResult> DeleteDef(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaDefPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = await _repo.AplicacaoidDaAreaAsync(item.Areaid);
                if (appId != claimAppId) return Forbid();
            }
            await _repo.RemoverDefAsync(item);
            return Ok();
        }

        // ── Submissão pública (sem autenticação) ─────────────────────────────

        [AllowAnonymous]
        [HttpPost("{formularioid}/submit")]
        public  async Task<IActionResult> Submit(string formularioid, [FromBody] Dictionary<string, string> campos)
        {
            var formulario = await _repo.BuscaFormularioPorIdAsync(formularioid);
            if (formulario == null) return NotFound();

            var item = new Formularionew
            {
                Formularioid = formularioid,
                Texto        = System.Text.Json.JsonSerializer.Serialize(campos),
                Nome         = campos.GetValueOrDefault("nome") ?? campos.GetValueOrDefault("Nome"),
                Email        = campos.GetValueOrDefault("email") ?? campos.GetValueOrDefault("Email"),
                Telefone     = campos.GetValueOrDefault("telefone") ?? campos.GetValueOrDefault("Telefone"),
                Ativo        = 1
            };
            await _repo.SubmeterAsync(item);
            return Ok();
        }

        // ── Respostas ────────────────────────────────────────────────────────

        [HttpGet("respostas")]
        public async Task<IEnumerable<Formularionew>> GetRespostas([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var filtroApp = acessoTotal ? aplicacaoid : claimAppId;
            return await _repo.ListaRespostasAsync(filtroApp);
        }

        [HttpPatch("respostas/{id}/ativo")]
        public  async Task<IActionResult> PatchAtivo(int id, [FromBody] int ativo)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaRespostaPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = await _repo.AplicacaoidDaRespostaAsync(item.Formularioid);
                if (appId != claimAppId) return Forbid();
            }
            item.Ativo = ativo;
            await _repo.AtualizarRespostaAtivoAsync(item);
            return Ok();
        }

        [HttpDelete("respostas/{id}")]
        public  async Task<IActionResult> DeleteResposta(int id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = await _repo.BuscaRespostaPorIdAsync(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = await _repo.AplicacaoidDaRespostaAsync(item.Formularioid);
                if (appId != claimAppId) return Forbid();
            }
            await _repo.RemoverRespostaAsync(item);
            return Ok();
        }
    }
}
