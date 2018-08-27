using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayMerchant.Client;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("payapidocs")]
    public class PublicApiDocsController : Controller
    {
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly ILog _log;

        public PublicApiDocsController(
            IPayMerchantClient payMerchantClient,
            ILogFactory logFactory)
        {
            _payMerchantClient = payMerchantClient;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        public async Task<IActionResult> PublicApiDocsAsync()
        {
            _log.Info($"Public API documentation opened by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            ViewBag.ApiaryDocsDomain = Startup.ApiaryDocsDomain;

            if (!User.Identity.IsAuthenticated) return View();

            try
            {
                ViewBag.MerchantId = User.GetMerchantId();
                var merchant = await _payMerchantClient.Api.GetByIdAsync(ViewBag.MerchantId);
                ViewBag.ApiKey = merchant.ApiKey;
            }
            catch (Exception e)
            {
                _log.Error(e, "Couldn't get info about merchant");
            }

            return View();
        }
    }
}
