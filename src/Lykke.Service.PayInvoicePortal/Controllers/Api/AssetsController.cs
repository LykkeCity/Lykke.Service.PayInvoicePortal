using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInvoicePortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/assets")]
    public class AssetsController : BaseController
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly ILog _log;

        public AssetsController(
            IPayInternalClient payInternalClient,
            IAssetsServiceWithCache assetsServiceWithCache,
            ILog log)
        {
            _payInternalClient = payInternalClient;
            _assetsServiceWithCache = assetsServiceWithCache;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var model = new List<ItemViewModel>();

            try
            {
                AvailableAssetsResponse response =
                    await _payInternalClient.ResolveAvailableAssetsAsync(MerchantId, AssetAvailabilityType.Settlement);

                foreach (string assetId in response.Assets)
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
