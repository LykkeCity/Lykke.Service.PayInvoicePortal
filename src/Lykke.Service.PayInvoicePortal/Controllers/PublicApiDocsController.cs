using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("payapidocs")]
    public class PublicApiDocsController : Controller
    {
        private readonly ILog _log;

        public PublicApiDocsController(
            ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        public IActionResult PublicApiDocs()
        {
            _log.Info($"Public API documentation opened by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            ViewBag.ApiaryDocsDomain = Startup.ApiaryDocsDomain;

            return View();
        }
    }
}
