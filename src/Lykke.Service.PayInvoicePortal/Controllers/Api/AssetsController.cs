using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoice.Client;
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
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly ILog _log;

        public AssetsController(
            IPayInvoiceClient payInvoiceClient,
            IAssetsServiceWithCache assetsServiceWithCache,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _assetsServiceWithCache = assetsServiceWithCache;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var model = new List<ItemViewModel>();

            try
            {
                IReadOnlyList<string> assets = await _payInvoiceClient.GetSettlementAssetsAsync(MerchantId);

                foreach (string assetId in assets)
                {
                    Asset asset = await _assetsServiceWithCache.TryGetAssetAsync(assetId);
                    model.Add(new ItemViewModel(assetId, asset.DisplayId));
                }
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(AssetsController), nameof(GetAsync), MerchantId, exception);
            }

            return Json(model);
        }
    }
}
