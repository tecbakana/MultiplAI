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
        public async Task<IEnumerable<object>> Get([FromQuery] string? aplicacaoid = null, [FromQuery] string? usuarioid = null) =>
            await _repo.ListaAsync(aplicacaoid, usuarioid);

        public class VincularModuloDto
        {
            public string? Usuarioid { get; set; }
            public string? Moduloid { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VincularModuloDto dto)
        {
            if (string.IsNullOrEmpty(dto.Usuarioid) || string.IsNullOrEmpty(dto.Moduloid))
                return BadRequest(new { message = "Selecione usuário e módulo." });

            if (await _repo.ExisteVinculoAsync(dto.Usuarioid, dto.Moduloid))
                return BadRequest(new { message = "Vínculo já existe." });

            var rel = new Relmodulousuario
            {
                Relacaoid = Guid.NewGuid().ToString(),
                Usuarioid = dto.Usuarioid,
                Moduloid  = dto.Moduloid
            };
            await _repo.CriarAsync(rel);
            return Ok(rel);
        }

        [HttpDelete("{relacaoid}")]
        public async Task<IActionResult> Delete(string relacaoid)
        {
            var item = await _repo.BuscaPorRelacaoidAsync(relacaoid);
            if (item == null) return NotFound();
            await _repo.RemoverAsync(item);
            return Ok();
        }
    }
}
