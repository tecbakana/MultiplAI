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
    public class ModulosController : Controller
    {
        private readonly IModuloRepositorio _repo;
        public ModulosController(IModuloRepositorio repo) { _repo = repo; }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        [HttpGet]
        public IEnumerable<Modulo> Get([FromQuery] string? usuarioid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();

            if (!string.IsNullOrEmpty(usuarioid))
                return _repo.ListaPorUsuario(usuarioid);

            if (acessoTotal)
                return _repo.ListaTodos();

            return _repo.ListaPorAplicacao(claimAppId!);
        }
    }
}
