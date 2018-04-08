using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
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
        private readonly ILog _log;

        public InvoicesController(
            IInvoiceService invoiceService,
            ILog log)
        {
            _invoiceService = invoiceService;
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

            var vm = new IndexViewModel
            {
                Invoice = invoice,
                BlockchainExplorerUrl = $"{Startup.BlockchainExplorerUrl.TrimEnd('/')}/address/{invoice.WalletAddress}"
            };

            return View(vm);
        }
    }
}
