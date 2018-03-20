using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Assets;
using Lykke.Service.PayInvoicePortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/assets")]
    public class AssetsController : BaseController
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public AssetsController(IPayInvoiceClient payInvoiceClient, ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            IReadOnlyList<ItemViewModel> model;

            try
            {
                IReadOnlyList<AssetModel> assets = await _payInvoiceClient.GetSettlementAssetsAsync();

                model = assets
                    .Select(o => new ItemViewModel(o.Id, o.Name))
                    .ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetAsync), MerchantId, exception);

                model = new List<ItemViewModel>();
            }

            return Json(model);
        }
    }
}
