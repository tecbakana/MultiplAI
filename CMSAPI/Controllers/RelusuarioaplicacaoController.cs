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
        public IActionResult Get([FromQuery] string? aplicacaoid = null, [FromQuery] string? usuarioid = null) =>
            Ok(_repo.Lista(aplicacaoid, usuarioid));

        public class NovoVinculoDto
        {
            public string Usuarioid { get; set; } = "";
            public string Aplicacaoid { get; set; } = "";
        }

        [HttpPost]
        public IActionResult Post([FromBody] NovoVinculoDto dto)
        {
            if (_repo.ExisteVinculo(dto.Usuarioid, dto.Aplicacaoid))
                return BadRequest(new { message = "Vínculo já existe." });

            var rel = new Relusuarioaplicacao
            {
                Relacaoid   = Guid.NewGuid().ToString(),
                Usuarioid   = dto.Usuarioid,
                Aplicacaoid = dto.Aplicacaoid
            };
            _repo.Criar(rel);
            return Ok(rel);
        }

        [HttpDelete("{relacaoid}")]
        public IActionResult Delete(string relacaoid)
        {
            var rel = _repo.BuscaPorRelacaoid(relacaoid);
            if (rel == null) return NotFound();
            _repo.Remover(rel);
            return Ok();
        }
    }
}
