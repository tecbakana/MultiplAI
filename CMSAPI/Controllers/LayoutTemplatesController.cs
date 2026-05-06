using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class LayoutTemplatesController : ControllerBase
    {
        private readonly ILayoutTemplateRepositorio _repo;
        public LayoutTemplatesController(ILayoutTemplateRepositorio repo) { _repo = repo; }

        private bool IsAdmin() => User.FindFirstValue("acessoTotal") == "True";

        [HttpGet]
        public IActionResult Get() => Ok(_repo.Lista());

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var t = _repo.BuscaPorId(id);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpGet("padrao")]
        public IActionResult GetPadrao([FromQuery] string tipo = "home")
        {
            var t = _repo.BuscaPadrao(tipo);
            return t == null ? NotFound() : Ok(t);
        }

        public class SalvarTemplateDto
        {
            public string Nome { get; set; } = "";
            public string? Descricao { get; set; }
            public string Tipo { get; set; } = "home";
            public string Layout { get; set; } = "";
            public bool Padrao { get; set; }
        }

        [HttpPost]
        public IActionResult Post([FromBody] SalvarTemplateDto dto)
        {
            if (!IsAdmin()) return Forbid();

            try { JsonDocument.Parse(dto.Layout); }
            catch { return BadRequest(new { erro = "Layout JSON inválido." }); }

            if (dto.Padrao)
                _repo.DesmarcarPadraoDoTipo(dto.Tipo, null);

            var item = new LayoutTemplate
            {
                Templateid   = Guid.NewGuid().ToString(),
                Nome         = dto.Nome,
                Descricao    = dto.Descricao,
                Tipo         = dto.Tipo,
                Layout       = dto.Layout,
                Padrao       = dto.Padrao,
                Datainclusao = DateTime.UtcNow
            };
            _repo.Criar(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] SalvarTemplateDto dto)
        {
            if (!IsAdmin()) return Forbid();

            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();

            if (dto.Padrao)
                _repo.DesmarcarPadraoDoTipo(dto.Tipo, id);

            item.Nome      = dto.Nome;
            item.Descricao = dto.Descricao;
            item.Tipo      = dto.Tipo;
            item.Layout    = dto.Layout;
            item.Padrao    = dto.Padrao;
            _repo.Atualizar(item);
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            if (!IsAdmin()) return Forbid();
            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();
            _repo.Remover(item);
            return Ok();
        }
    }
}
