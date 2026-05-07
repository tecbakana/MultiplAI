using Microsoft.AspNetCore.Mvc;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("vinculos")]
    public class RelusuarioaplicacaoController : Controller
    {
        private readonly IVinculoUsuarioAplicacaoRepositorio _repo;
        public RelusuarioaplicacaoController(IVinculoUsuarioAplicacaoRepositorio repo) { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? aplicacaoid = null, [FromQuery] string? usuarioid = null) =>
            Ok(await _repo.ListaAsync(aplicacaoid, usuarioid));

        public class NovoVinculoDto
        {
            public string Usuarioid { get; set; } = "";
            public string Aplicacaoid { get; set; } = "";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] NovoVinculoDto dto)
        {
            if (await _repo.ExisteVinculoAsync(dto.Usuarioid, dto.Aplicacaoid))
                return BadRequest(new { message = "Vínculo já existe." });

            var rel = new Relusuarioaplicacao
            {
                Relacaoid   = Guid.NewGuid().ToString(),
                Usuarioid   = dto.Usuarioid,
                Aplicacaoid = dto.Aplicacaoid
            };
            await _repo.CriarAsync(rel);
            return Ok(rel);
        }

        [HttpDelete("{relacaoid}")]
        public async Task<IActionResult> Delete(string relacaoid)
        {
            var rel = await _repo.BuscaPorRelacaoidAsync(relacaoid);
            if (rel == null) return NotFound();
            await _repo.RemoverAsync(rel);
            return Ok();
        }
    }
}
