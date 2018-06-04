using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client.Models;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly ILog _log;

        public AssetsController(
            IAssetService assetService,
            ILog log)
        {
            _assetService = assetService;
            _log = log;
        }

        [Authorize]
        [HttpGet]
        [Route("/api/assets")]
        public async Task<IActionResult> GetAssetsAsync()
        {
            IReadOnlyDictionary<string, string> assets = await _assetService.GetSettlementAssetsAsync(User.GetMerchantId());

            var model = assets.Select(o => new AssetItemViewModel(o.Key, o.Value));
            
            return Json(model);
        }

        [HttpGet]
        [Route("/api/paymentAssets")]
        public async Task<IActionResult> GetPaymentAssetsAsync(string merchantId, string settlementAssetId)
        {
            IReadOnlyDictionary<string, string> assets = await _assetService.GetPaymentAssetsAsync(merchantId, settlementAssetId);

            IReadOnlyDictionary<string, BlockchainType> assetsNetwork = await _assetService.GetAssetsNetworkAsync();

            var model = assets.Select(o => new AssetItemViewModel
            {
                Id = o.Key,
                Title = o.Value,
                Network = assetsNetwork.TryGetValue(o.Key, out var network) ? network : BlockchainType.None
            });

            return Json(model);
        }
    }
}
