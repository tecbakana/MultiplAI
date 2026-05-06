using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMSXData.Models;
using ICMSX;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IAcessoRepositorio _repo;
        private readonly IConfiguration _config;

        public AuthController(IAcessoRepositorio repo, IConfiguration config)
        {
            _repo   = repo;
            _config = config;
        }

        public class LoginRequest { public string Apelido { get; set; } = ""; public string Senha { get; set; } = ""; }

        public class SignupRequest
        {
            public string Nome { get; set; } = "";
            public string Sobrenome { get; set; } = "";
            public string Apelido { get; set; } = "";
            public string Senha { get; set; } = "";
            public string AppNome { get; set; } = "";
            public string AppUrl { get; set; } = "";
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupRequest req)
        {
            if (!_repo.ApelidoDisponivel(req.Apelido))
                return BadRequest(new { message = "Login já está em uso." });

            if (!_repo.UrlDisponivel(req.AppUrl))
                return BadRequest(new { message = "URL da aplicação já está em uso." });

            var userId = Guid.NewGuid().ToString();
            var appId  = Guid.NewGuid().ToString();

            _repo.CriarConta(
                new Usuario
                {
                    Userid       = userId,
                    Nome         = req.Nome,
                    Sobrenome    = req.Sobrenome,
                    Apelido      = req.Apelido,
                    Senha        = req.Senha,
                    Ativo        = 0,
                    Datainclusao = DateTime.UtcNow
                },
                new Aplicacao
                {
                    Aplicacaoid     = appId,
                    Nome            = req.AppNome,
                    Url             = req.AppUrl,
                    Idusuarioinicio = userId,
                    Datainicio      = DateTime.UtcNow,
                    Isactive        = false,
                    Layoutchoose    = "_Layout.cshtml"
                },
                new Relusuarioaplicacao
                {
                    Usuarioid   = userId,
                    Aplicacaoid = appId,
                    Relacaoid   = Guid.NewGuid().ToString()
                }
            );

            return Ok(new { message = "Cadastro realizado com sucesso." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var resultado = _repo.Login(req.Apelido, req.Senha);
            if (resultado == null)
                return Unauthorized(new { message = "Login ou senha inválidos" });

            var claims = new[]
            {
                new Claim("userid",      resultado.Usuario.Userid ?? ""),
                new Claim("apelido",     resultado.Usuario.Apelido ?? ""),
                new Claim("nome",        resultado.Usuario.Nome ?? ""),
                new Claim("aplicacaoid", resultado.Aplicacaoid ?? ""),
                new Claim("acessoTotal", resultado.AcessoTotal.ToString()),
                new Claim("isDemo",      resultado.IsDemo.ToString())
            };

            var token = GerarToken(claims, 8);

            return Ok(new
            {
                token       = new JwtSecurityTokenHandler().WriteToken(token),
                userid      = resultado.Usuario.Userid,
                nome        = resultado.Usuario.Nome,
                apelido     = resultado.Usuario.Apelido,
                acessoTotal = resultado.AcessoTotal,
                grupos      = resultado.NomesGrupos,
                aplicacaoid = resultado.Aplicacaoid,
                isDemo      = resultado.IsDemo
            });
        }

        [HttpPost("demo-login")]
        public IActionResult DemoLogin()
        {
            var resultado = _repo.DemoLogin();
            if (resultado == null)
                return NotFound(new { message = "Tenant demo não configurado. Execute cmsxDB.tenant_demo.sql." });

            _repo.ResetarTenantDemo(resultado.Aplicacaoid);

            var claims = new[]
            {
                new Claim("userid",      resultado.Usuario.Userid ?? ""),
                new Claim("apelido",     resultado.Usuario.Apelido ?? ""),
                new Claim("nome",        "Demo"),
                new Claim("aplicacaoid", resultado.Aplicacaoid),
                new Claim("acessoTotal", "False"),
                new Claim("isDemo",      "True")
            };

            var token = GerarToken(claims, 2);

            return Ok(new
            {
                token       = new JwtSecurityTokenHandler().WriteToken(token),
                userid      = resultado.Usuario.Userid,
                nome        = "Demo",
                apelido     = "demo",
                acessoTotal = false,
                grupos      = new string[0],
                aplicacaoid = resultado.Aplicacaoid,
                isDemo      = true
            });
        }

        private JwtSecurityToken GerarToken(Claim[] claims, int horas)
        {
            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            return new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(horas),
                signingCredentials: creds
            );
        }
    }
}
