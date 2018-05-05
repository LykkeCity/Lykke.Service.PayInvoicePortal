using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
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
            IReadOnlyList<Asset> assets = await _assetService.GetSettlementAssetsAsync(User.GetMerchantId());

            var model = assets.Select(o => new ItemViewModel(o.Id, o.DisplayId ?? "empty"));
            
            return Json(model);
        }

        [HttpGet]
        [Route("/api/paymentAssets")]
        public async Task<IActionResult> GetPaymentAssetsAsync(string merchantId, string settlementAssetId)
        {
            IReadOnlyList<Asset> assets = await _assetService.GetPaymentAssetsAsync(merchantId, settlementAssetId);

            var model = assets.Select(o => new ItemViewModel(o.Id, o.DisplayId ?? "empty"));

            return Json(model);
        }
    }
}
