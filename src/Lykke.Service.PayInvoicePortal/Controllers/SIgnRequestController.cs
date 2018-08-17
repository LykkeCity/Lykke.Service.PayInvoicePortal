using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInvoicePortal.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    [Route("signrequest")]
    public class SignRequestController : Controller
    {
        private readonly ILog _log;

        public SignRequestController(
            ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        public IActionResult SignRequest()
        {
            _log.Info($"SignRequest opened by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            ViewBag.MerchantId = User.GetMerchantId();

            return View();
        }
    }
}
