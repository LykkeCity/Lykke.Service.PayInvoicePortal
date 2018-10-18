using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client.Models;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;

        public AssetsController(
            IAssetService assetService)
        {
            _assetService = assetService;
        }

        [Authorize]
        [HttpGet]
        [Route("/api/assets/baseAsset")]
        public async Task<IActionResult> GetBaseAssetId()
        {
            Asset baseAsset = await _assetService.GetBaseAssetOrDefault(User.GetMerchantId());

            return Ok(Mapper.Map<AssetModel>(baseAsset));
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

            var assetItemViewModels = assets.Select(o => new AssetItemViewModel
            {
                Id = o.Key,
                Title = o.Value
            });

            foreach (var assetItemViewModel in assetItemViewModels)
            {
                assetItemViewModel.Network = await _assetService.GetAssetNetworkAsync(assetItemViewModel.Id);
            }

            return Json(assetItemViewModels);
        }

        [Authorize]
        [HttpGet]
        [Route("/api/paymentAssetsOfMerchant")]
        public async Task<IActionResult> GetPaymentAssetsOfMerchantAsync()
        {
            IReadOnlyDictionary<string, string> assets = await _assetService.GetPaymentAssetsAsync(User.GetMerchantId());

            var model = assets.Select(o => new AssetItemViewModel(o.Key, o.Value));

            return Json(model);
        }
    }
}
