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
    public class CategoriasController : Controller
    {
        private readonly ICategoriaRepositorio _repo;
        public CategoriasController(ICategoriaRepositorio repo) { _repo = repo; }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        [HttpGet]
        public IEnumerable<Caterium> Get([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var filtro = acessoTotal ? aplicacaoid : claimAppId;
            return _repo.Lista(filtro);
        }

        public class NovaCategoriaDto
        {
            public string? Nome { get; set; }
            public string? Descricao { get; set; }
            public int? Tipocateria { get; set; }
            public string? Cateriaidpai { get; set; }
            public string? Aplicacaoid { get; set; }
        }

        [HttpPost]
        public IActionResult Post([FromBody] NovaCategoriaDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = new Caterium
            {
                Cateriaid    = Guid.NewGuid().ToString(),
                Nome         = dto.Nome,
                Descricao    = dto.Descricao,
                Tipocateria  = dto.Tipocateria,
                Cateriaidpai = dto.Cateriaidpai,
                Aplicacaoid  = acessoTotal ? dto.Aplicacaoid : claimAppId
            };
            _repo.Criar(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] NovaCategoriaDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
            item.Nome        = dto.Nome;
            item.Descricao   = dto.Descricao;
            item.Tipocateria = dto.Tipocateria;
            item.Cateriaidpai = dto.Cateriaidpai;
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
