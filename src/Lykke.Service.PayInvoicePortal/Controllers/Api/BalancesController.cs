using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models.Balances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/balances")]
    public class BalancesController : Controller
    {
        private readonly IBalanceService _balanceService;
        private readonly ILog _log;

        public BalancesController(IBalanceService balanceService, ILog log)
        {
            _balanceService = balanceService;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            Balance balance = await _balanceService.GetAsync(User.GetMerchantId());

            var model = new BalanceModel
            {
                Value = balance.Value,
                Currency = balance.Currency
            };

            return Json(model);
        }
    }
}
