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
        public  async Task<IActionResult> Get([FromQuery] string? aplicacaoid = null)
        {
            var acessoTotal = User.FindFirstValue("acessoTotal") == "True";
            var claimAppId  = User.FindFirstValue("aplicacaoid");

            if (acessoTotal && string.IsNullOrEmpty(aplicacaoid))
                return Ok(await _repo.TotaisGlobaisAsync());

            var filtroId = acessoTotal ? aplicacaoid : claimAppId;
            if (string.IsNullOrEmpty(filtroId)) return Forbid();

            return Ok(await _repo.TotaisPorAplicacaoAsync(filtroId));
        }
    }
}
