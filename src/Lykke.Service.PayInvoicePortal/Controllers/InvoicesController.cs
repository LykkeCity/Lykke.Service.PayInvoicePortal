using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.PayInternal.Client.Models;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    [Route("/invoices")]
    public class InvoicesController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IAssetService _assetService;
        private readonly ILog _log;

        public InvoicesController(
            IInvoiceService invoiceService,
            IAssetService assetService,
            ILog log)
        {
            _invoiceService = invoiceService;
            _assetService = assetService;
            _log = log;
        }

        [HttpGet]
        [Route("{invoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            var invoiceTask = _invoiceService.GetByIdAsync(invoiceId);
            var filesTask = _invoiceService.GetFilesAsync(invoiceId);
            var historyTask = _invoiceService.GetHistoryAsync(User.GetMerchantId(), invoiceId);

            await Task.WhenAll(invoiceTask, filesTask, historyTask);

            var invoice = Mapper.Map<InvoiceModel>(invoiceTask.Result);
            invoice.Files = Mapper.Map<List<FileModel>>(filesTask.Result);
            invoice.History = Mapper.Map<List<HistoryItemModel>>(historyTask.Result);

            IReadOnlyDictionary<string, BlockchainType> assetsNetwork = await _assetService.GetAssetsNetworkAsync();
            invoice.PaymentAssetNetwork = assetsNetwork.TryGetValue(invoice.PaymentAsset, out var network)
                ? network.ToString() : string.Empty;

            var vm = new IndexViewModel
            {
                Invoice = invoice,
                BlockchainExplorerUrl = Startup.BlockchainExplorerUrl.TrimEnd('/'),
                EthereumBlockchainExplorerUrl = Startup.EthereumBlockchainExplorerUrl.TrimEnd('/')
            };

            return View(vm);
        }
    }
}
