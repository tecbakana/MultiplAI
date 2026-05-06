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
        public IEnumerable<object> GetDefs([FromQuery] string? areaid = null, [FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var filtroApp = acessoTotal ? aplicacaoid : claimAppId;
            return _repo.ListaDefs(filtroApp, areaid)
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
        public IActionResult PostDef([FromBody] FormularioDefDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            if (!acessoTotal && !string.IsNullOrEmpty(dto.Areaid))
            {
                var appId = _repo.AplicacaoidDaArea(dto.Areaid);
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
            _repo.CriarDef(item);
            return Ok(item);
        }

        [HttpPut("defs/{id}")]
        public IActionResult PutDef(string id, [FromBody] FormularioDefDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaDefPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = _repo.AplicacaoidDaArea(item.Areaid);
                if (appId != claimAppId) return Forbid();
            }
            item.Nome       = dto.Nome ?? item.Nome;
            item.Valor      = dto.Valor;
            item.Areaid     = dto.Areaid;
            item.Categoriaid = dto.Categoriaid;
            _repo.AtualizarDef(item);
            return Ok(item);
        }

        [HttpDelete("defs/{id}")]
        public IActionResult DeleteDef(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaDefPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = _repo.AplicacaoidDaArea(item.Areaid);
                if (appId != claimAppId) return Forbid();
            }
            _repo.RemoverDef(item);
            return Ok();
        }

        // ── Submissão pública (sem autenticação) ─────────────────────────────

        [AllowAnonymous]
        [HttpPost("{formularioid}/submit")]
        public IActionResult Submit(string formularioid, [FromBody] Dictionary<string, string> campos)
        {
            var formulario = _repo.BuscaFormularioPorId(formularioid);
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
            _repo.Submeter(item);
            return Ok();
        }

        // ── Respostas ────────────────────────────────────────────────────────

        [HttpGet("respostas")]
        public IEnumerable<Formularionew> GetRespostas([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var filtroApp = acessoTotal ? aplicacaoid : claimAppId;
            return _repo.ListaRespostas(filtroApp);
        }

        [HttpPatch("respostas/{id}/ativo")]
        public IActionResult PatchAtivo(int id, [FromBody] int ativo)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaRespostaPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = _repo.AplicacaoidDaResposta(item.Formularioid);
                if (appId != claimAppId) return Forbid();
            }
            item.Ativo = ativo;
            _repo.AtualizarRespostaAtivo(item);
            return Ok();
        }

        [HttpDelete("respostas/{id}")]
        public IActionResult DeleteResposta(int id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaRespostaPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = _repo.AplicacaoidDaResposta(item.Formularioid);
                if (appId != claimAppId) return Forbid();
            }
            _repo.RemoverResposta(item);
            return Ok();
        }
    }
}
