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
    public class UsuariosController : Controller
    {
        private readonly IUsuarioRepositorio _repo;
        public UsuariosController(IUsuarioRepositorio repo) { _repo = repo; }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        [HttpGet]
        public IEnumerable<object> Get([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();

            if (acessoTotal)
            {
                if (!string.IsNullOrEmpty(aplicacaoid))
                    return _repo.ListaPorAplicacao(aplicacaoid);
                return _repo.ListaTodos();
            }

            return _repo.ListaPorAplicacao(claimAppId!);
        }

        public class NovoUsuarioDto
        {
            public string Nome { get; set; } = "";
            public string Sobrenome { get; set; } = "";
            public string? Apelido { get; set; }
            public string? Senha { get; set; }
        }

        [HttpPost]
        public IActionResult Post([FromBody] NovoUsuarioDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();

            var usuario = new Usuario
            {
                Userid       = Guid.NewGuid().ToString(),
                Nome         = dto.Nome,
                Sobrenome    = dto.Sobrenome,
                Apelido      = dto.Apelido,
                Senha        = dto.Senha,
                Ativo        = 1,
                Datainclusao = DateTime.UtcNow
            };

            Relusuarioaplicacao? vinculo = null;
            if (!acessoTotal && !string.IsNullOrEmpty(claimAppId))
            {
                vinculo = new Relusuarioaplicacao
                {
                    Relacaoid   = Guid.NewGuid().ToString(),
                    Usuarioid   = usuario.Userid,
                    Aplicacaoid = claimAppId
                };
            }

            _repo.Criar(usuario, vinculo);
            return Ok(usuario);
        }

        public class EditarUsuarioDto
        {
            public string Nome { get; set; } = "";
            public string Sobrenome { get; set; } = "";
            public string? Apelido { get; set; }
            public string? Senha { get; set; }
            public byte? Ativo { get; set; }
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] EditarUsuarioDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var user = _repo.BuscaPorId(id);
            if (user == null) return NotFound();

            if (!acessoTotal && !_repo.PertenceAplicacao(id, claimAppId!))
                return Forbid();

            user.Nome      = dto.Nome;
            user.Sobrenome = dto.Sobrenome;
            user.Apelido   = dto.Apelido;
            user.Ativo     = dto.Ativo;
            if (!string.IsNullOrWhiteSpace(dto.Senha))
                user.Senha = dto.Senha;
            _repo.Atualizar(user);
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var user = _repo.BuscaPorId(id);
            if (user == null) return NotFound();

            if (!acessoTotal && !_repo.PertenceAplicacao(id, claimAppId!))
                return Forbid();

            _repo.Remover(user);
            return Ok();
        }
    }
}
