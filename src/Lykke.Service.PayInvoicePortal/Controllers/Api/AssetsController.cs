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
    [Authorize]
    [Route("/api/assets")]
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

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            IReadOnlyList<Asset> assets = await _assetService.GetSettlementAssetsAsync(User.GetMerchantId());

            var model = assets.Select(o => new ItemViewModel(o.Id, o.DisplayId));
            
            return Json(model);
        }
    }
}
