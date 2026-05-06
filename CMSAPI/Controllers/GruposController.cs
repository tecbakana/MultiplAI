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
    public class GruposController : Controller
    {
        private readonly IGrupoRepositorio _repo;
        public GruposController(IGrupoRepositorio repo) { _repo = repo; }

        private bool IsAdmin() => User.FindFirstValue("acessoTotal") == "True";

        [HttpGet]
        public IActionResult Get()
        {
            if (!IsAdmin()) return Forbid();
            return Ok(_repo.Lista());
        }

        [HttpGet("{id}/usuarios")]
        public IActionResult GetUsuarios(string id)
        {
            if (!IsAdmin()) return Forbid();
            return Ok(_repo.UsuariosPorGrupo(id));
        }

        public class NovoGrupoDto
        {
            public string Nome { get; set; } = "";
            public string? Descricao { get; set; }
            public bool Acessototal { get; set; }
        }

        [HttpPost]
        public IActionResult Post([FromBody] NovoGrupoDto dto)
        {
            if (!IsAdmin()) return Forbid();
            var item = new Grupo
            {
                Grupoid     = Guid.NewGuid().ToString(),
                Nome        = dto.Nome,
                Descricao   = dto.Descricao,
                Acessototal = dto.Acessototal
            };
            _repo.Criar(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] NovoGrupoDto dto)
        {
            if (!IsAdmin()) return Forbid();
            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();
            item.Nome        = dto.Nome;
            item.Descricao   = dto.Descricao;
            item.Acessototal = dto.Acessototal;
            _repo.Atualizar(item);
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            if (!IsAdmin()) return Forbid();
            var item = _repo.BuscaPorId(id);
            if (item == null) return NotFound();
            _repo.RemoverComVinculos(item);
            return Ok();
        }

        [HttpPost("{id}/usuarios")]
        public IActionResult AddUsuario(string id, [FromBody] string usuarioid)
        {
            if (!IsAdmin()) return Forbid();
            if (_repo.ExisteVinculoUsuario(id, usuarioid))
                return BadRequest(new { message = "Usuário já pertence a este grupo." });

            _repo.AdicionarUsuario(new Relusuariogrupo
            {
                Relacaoid = Guid.NewGuid().ToString(),
                Grupoid   = id,
                Usuarioid = usuarioid
            });
            return Ok();
        }

        [HttpDelete("{id}/usuarios/{relacaoid}")]
        public IActionResult RemoveUsuario(string id, string relacaoid)
        {
            if (!IsAdmin()) return Forbid();
            var rel = _repo.BuscaVinculoPorRelacaoid(relacaoid);
            if (rel == null) return NotFound();
            _repo.RemoverVinculoUsuario(rel);
            return Ok();
        }
    }
}
