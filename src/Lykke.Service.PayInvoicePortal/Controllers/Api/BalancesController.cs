using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoicePortal.Models.Balances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/balances")]
    public class BalancesController : BaseController
    {
        private const string AssetId = "CHF";

        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public BalancesController(IPayInvoiceClient payInvoiceClient, ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var model = new BalanceModel
            {
                Currency = AssetId
            };

            try
            {
                PayInvoice.Client.Models.Balances.BalanceModel balance = await _payInvoiceClient.GetBalanceAsync(MerchantId, AssetId);

                if (balance?.Balance != null)
                    model.Value = balance.Balance.Value;
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(BalancesController), nameof(GetAsync), MerchantId, exception);
            }

            return Json(model);
        }
    }
}
