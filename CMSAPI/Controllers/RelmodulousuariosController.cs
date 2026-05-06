using Microsoft.AspNetCore.Mvc;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("vinculosmodulo")]
    public class RelmodulousuariosController : Controller
    {
        private readonly IVinculoModuloUsuarioRepositorio _repo;
        public RelmodulousuariosController(IVinculoModuloUsuarioRepositorio repo) { _repo = repo; }

        [HttpGet]
        public IEnumerable<object> Get([FromQuery] string? aplicacaoid = null, [FromQuery] string? usuarioid = null) =>
            _repo.Lista(aplicacaoid, usuarioid);

        public class VincularModuloDto
        {
            public string? Usuarioid { get; set; }
            public string? Moduloid { get; set; }
        }

        [HttpPost]
        public IActionResult Post([FromBody] VincularModuloDto dto)
        {
            if (string.IsNullOrEmpty(dto.Usuarioid) || string.IsNullOrEmpty(dto.Moduloid))
                return BadRequest(new { message = "Selecione usuário e módulo." });

            if (_repo.ExisteVinculo(dto.Usuarioid, dto.Moduloid))
                return BadRequest(new { message = "Vínculo já existe." });

            var rel = new Relmodulousuario
            {
                Relacaoid = Guid.NewGuid().ToString(),
                Usuarioid = dto.Usuarioid,
                Moduloid  = dto.Moduloid
            };
            _repo.Criar(rel);
            return Ok(rel);
        }

        [HttpDelete("{relacaoid}")]
        public IActionResult Delete(string relacaoid)
        {
            var item = _repo.BuscaPorRelacaoid(relacaoid);
            if (item == null) return NotFound();
            _repo.Remover(item);
            return Ok();
        }
    }
}
