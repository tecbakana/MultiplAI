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
    public class AreasController : Controller
    {
        private readonly IAreasRepositorio _repo;
        public AreasController(IAreasRepositorio repo) { _repo = repo; }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        [HttpGet]
        public IEnumerable<Area> Get([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var filtro = acessoTotal ? aplicacaoid : claimAppId;
            return _repo.Lista(filtro);
        }

        public class NovaAreaDto
        {
            public string? Nome { get; set; }
            public string? Url { get; set; }
            public string? Descricao { get; set; }
            public string? Areaidpai { get; set; }
            public int? Posicao { get; set; }
            public int? Tipoarea { get; set; }
            public string? Aplicacaoid { get; set; }
        }

        [HttpPost]
        public IActionResult Post([FromBody] NovaAreaDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = new Area
            {
                Areaid      = Guid.NewGuid().ToString(),
                Nome        = dto.Nome,
                Url         = dto.Url,
                Descricao   = dto.Descricao,
                Areaidpai   = dto.Areaidpai,
                Posicao     = dto.Posicao,
                Tipoarea    = dto.Tipoarea,
                Aplicacaoid = acessoTotal ? dto.Aplicacaoid : claimAppId,
                Datainicial = DateTime.UtcNow
            };
            _repo.Criar(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] NovaAreaDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
            item.Nome      = dto.Nome;
            item.Url       = dto.Url;
            item.Descricao = dto.Descricao;
            item.Areaidpai = dto.Areaidpai;
            item.Posicao   = dto.Posicao;
            item.Tipoarea  = dto.Tipoarea;
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
    }
}
