using ICMSX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepositorio _repo;
        public DashboardController(IDashboardRepositorio repo) { _repo = repo; }

        [HttpGet]
        [Authorize]
        public IActionResult Get([FromQuery] string? aplicacaoid = null)
        {
            var acessoTotal = User.FindFirstValue("acessoTotal") == "True";
            var claimAppId  = User.FindFirstValue("aplicacaoid");

            if (acessoTotal && string.IsNullOrEmpty(aplicacaoid))
                return Ok(_repo.TotaisGlobais());

            var filtroId = acessoTotal ? aplicacaoid : claimAppId;
            if (string.IsNullOrEmpty(filtroId)) return Forbid();

            return Ok(_repo.TotaisPorAplicacao(filtroId));
        }
    }
}
