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
    public class ConteudosController : Controller
    {
        private readonly IConteudoRepositorio _repo;
        public ConteudosController(IConteudoRepositorio repo) { _repo = repo; }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        [HttpGet]
        public IEnumerable<Conteudo> Get([FromQuery] string? areaid = null, [FromQuery] string? cateriaid = null, [FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var filtroApp = acessoTotal ? aplicacaoid : claimAppId;
            return _repo.Lista(filtroApp, areaid, cateriaid);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = _repo.AplicacaoidDaArea(item.Areaid);
                if (appId != claimAppId) return Forbid();
            }
            return Ok(item);
        }

        public class NovoConteudoDto
        {
            public string? Titulo { get; set; }
            public string? Texto { get; set; }
            public string? Autor { get; set; }
            public string? Areaid { get; set; }
            public string? Cateriaid { get; set; }
            public DateTime? Datafinal { get; set; }
        }

        [HttpPost]
        public IActionResult Post([FromBody] NovoConteudoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            if (!acessoTotal && !string.IsNullOrEmpty(dto.Areaid))
            {
                var appId = _repo.AplicacaoidDaArea(dto.Areaid);
                if (appId != claimAppId) return Forbid();
            }

            var item = new Conteudo
            {
                Conteudoid   = Guid.NewGuid().ToString(),
                Titulo       = dto.Titulo,
                Texto        = dto.Texto,
                Autor        = dto.Autor,
                Areaid       = dto.Areaid,
                Cateriaid    = dto.Cateriaid,
                Datafinal    = dto.Datafinal,
                Datainclusao = DateTime.UtcNow
            };
            _repo.Criar(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] Conteudo item)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var existing = _repo.BuscaPorId(id);
            if (existing == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = _repo.AplicacaoidDaArea(existing.Areaid);
                if (appId != claimAppId) return Forbid();
            }
            existing.Titulo    = item.Titulo;
            existing.Texto     = item.Texto;
            existing.Autor     = item.Autor;
            existing.Cateriaid = item.Cateriaid;
            existing.Areaid    = item.Areaid;
            existing.Datafinal = item.Datafinal;
            _repo.Atualizar(existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal)
            {
                var appId = _repo.AplicacaoidDaArea(item.Areaid);
                if (appId != claimAppId) return Forbid();
            }
            _repo.Remover(item);
            return Ok();
        }
    }
}
