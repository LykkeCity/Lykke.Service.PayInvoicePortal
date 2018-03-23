using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("/invoices")]
    public class InvoicesController : BaseController
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ILog _log;

        public InvoicesController(
            IPayInvoiceClient payInvoiceClient,
            IAssetsServiceWithCache assetsService,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _assetsService = assetsService;
            _log = log;
        }

        [HttpGet]
        [Route("{invoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            PayInvoice.Client.Models.Invoice.InvoiceModel invoice =
                await _payInvoiceClient.GetInvoiceAsync(invoiceId);
            IEnumerable<FileInfoModel> invoiceFiles = await _payInvoiceClient.GetFilesAsync(invoice.Id);
            Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoice.SettlementAssetId);

            var result = new InvoiceModel
            {
                Id = invoice.Id,
                Number = invoice.Number,
                ClientEmail = invoice.ClientEmail,
                ClientName = invoice.ClientName,
                Amount = (double)invoice.Amount,
                DueDate = invoice.DueDate,
                Status = invoice.Status.ToString(),
                Currency = settlementAsset.DisplayId,
                CreatedDate = invoice.CreatedDate,
                Files = invoiceFiles
                    .Select(o => new FileModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Size = o.Size
                    })
                    .ToList()
            };

            return View(result);
        }
    }
}
