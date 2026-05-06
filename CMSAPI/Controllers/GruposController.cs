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
        public  async Task<IActionResult> Get()
        {
            if (!IsAdmin()) return Forbid();
            return Ok(await _repo.ListaAsync());
        }

        [HttpGet("{id}/usuarios")]
        public  async Task<IActionResult> GetUsuarios(string id)
        {
            if (!IsAdmin()) return Forbid();
            return Ok(await _repo.UsuariosPorGrupoAsync(id));
        }

        public class NovoGrupoDto
        {
            public string Nome { get; set; } = "";
            public string? Descricao { get; set; }
            public bool Acessototal { get; set; }
        }

        [HttpPost]
        public  async Task<IActionResult> Post([FromBody] NovoGrupoDto dto)
        {
            if (!IsAdmin()) return Forbid();
            var item = new Grupo
            {
                Grupoid     = Guid.NewGuid().ToString(),
                Nome        = dto.Nome,
                Descricao   = dto.Descricao,
                Acessototal = dto.Acessototal
            };
            await _repo.CriarAsync(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        public  async Task<IActionResult> Put(string id, [FromBody] NovoGrupoDto dto)
        {
            if (!IsAdmin()) return Forbid();
            var item = await _repo.BuscaPorIdAsync(id);
            if (item == null) return NotFound();
            item.Nome        = dto.Nome;
            item.Descricao   = dto.Descricao;
            item.Acessototal = dto.Acessototal;
            await _repo.AtualizarAsync(item);
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public  async Task<IActionResult> Delete(string id)
        {
            if (!IsAdmin()) return Forbid();
            var item = await _repo.BuscaPorIdAsync(id);
            if (item == null) return NotFound();
            await _repo.RemoverComVinculosAsync(item);
            return Ok();
        }

        [HttpPost("{id}/usuarios")]
        public  async Task<IActionResult> AddUsuario(string id, [FromBody] string usuarioid)
        {
            if (!IsAdmin()) return Forbid();
            if (await _repo.ExisteVinculoUsuarioAsync(id, usuarioid))
                return BadRequest(new { message = "Usuário já pertence a este grupo." });

            await _repo.AdicionarUsuarioAsync(new Relusuariogrupo
            {
                Relacaoid = Guid.NewGuid().ToString(),
                Grupoid   = id,
                Usuarioid = usuarioid
            });
            return Ok();
        }

        [HttpDelete("{id}/usuarios/{relacaoid}")]
        public  async Task<IActionResult> RemoveUsuario(string id, string relacaoid)
        {
            if (!IsAdmin()) return Forbid();
            var rel = await _repo.BuscaVinculoPorRelacaoidAsync(relacaoid);
            if (rel == null) return NotFound();
            await _repo.RemoverVinculoUsuarioAsync(rel);
            return Ok();
        }
    }
}
