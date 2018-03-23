using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/invoices")]
    public class InvoicesController : BaseController
    {
        private const string PaymentAssetId = "BTC";

        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IInvoiceService _invoiceService;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ILog _log;

        public InvoicesController(
            IPayInvoiceClient payInvoiceClient,
            IInvoiceService invoiceService,
            IAssetsServiceWithCache assetsService,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _invoiceService = invoiceService;
            _assetsService = assetsService;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvociesAsync(
            string searchValue,
            Period period,
            List<PayInvoice.Client.Models.Invoice.InvoiceStatus> status,
            string sortField,
            bool sortAscending,
            int skip,
            int take)
        {
            InvoiceSource source = await _invoiceService.GetAsync(
                MerchantId,
                status,
                period,
                searchValue,
                sortField,
                sortAscending,
                skip,
                take);

            var model = new ListModel
            {
                Total = source.Total,
                CountPerStatus = source.CountPerStatus.ToDictionary(o => o.Key.ToString(), o => o.Value)
            };

            foreach (PayInvoice.Client.Models.Invoice.InvoiceModel item in source.Items)
            {
                Asset settlementAsset = await _assetsService.TryGetAssetAsync(item.SettlementAssetId);

                model.Items.Add(new ListItemModel
                {
                    Id = item.Id,
                    Number = item.Number,
                    ClientEmail = item.ClientEmail,
                    ClientName = item.ClientName,
                    Amount = (double)item.Amount,
                    DueDate = item.DueDate,
                    Status = item.Status.ToString(),
                    SettlementAsset = settlementAsset.DisplayId,
                    SettlementAssetAccuracy = settlementAsset.Accuracy,
                    CreatedDate = item.CreatedDate
                });
            }

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(CreateInvoiceModel model, IFormFileCollection files)
        {
            PayInvoice.Client.Models.Invoice.InvoiceModel invoice;

            if (model.IsDraft)
            {
                invoice = await _payInvoiceClient.CreateDraftInvoiceAsync(MerchantId, new PayInvoice.Client.Models.Invoice.CreateDraftInvoiceModel
                {
                    EmployeeId = EmployeeId,
                    Number = model.Number,
                    ClientName = model.Client,
                    ClientEmail = model.Email,
                    Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                    SettlementAssetId = model.SettlementAsset,
                    PaymentAssetId = PaymentAssetId,
                    DueDate = model.DueDate
                    // TODO: NOTE
                });
            }
            else
            {
                invoice = await _payInvoiceClient.CreateInvoiceAsync(MerchantId, new PayInvoice.Client.Models.Invoice.CreateInvoiceModel
                {
                    EmployeeId = EmployeeId,
                    Number = model.Number,
                    ClientName = model.Client,
                    ClientEmail = model.Email,
                    Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                    SettlementAssetId = model.SettlementAsset,
                    PaymentAssetId = PaymentAssetId,
                    DueDate = model.DueDate
                    // TODO: NOTE
                });
            }

            if (files != null)
            {
                foreach (IFormFile formFile in files)
                {
                    byte[] content;

                    using (var ms = new MemoryStream())
                    {
                        formFile.CopyTo(ms);
                        content = ms.ToArray();
                    }

                    await _payInvoiceClient.UploadFileAsync(invoice.Id, content, formFile.FileName, formFile.ContentType);
                }
            }

            IEnumerable<FileInfoModel> invoiceFiles = await _payInvoiceClient.GetFilesAsync(invoice.Id);
            Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoice.SettlementAssetId);

            var result = new InvoiceModel
            {
                Id = invoice.Id,
                Number = invoice.Number,
                ClientEmail = invoice.ClientEmail,
                ClientName = invoice.ClientName,
                Amount = (double) invoice.Amount,
                DueDate = invoice.DueDate,
                Status = invoice.Status.ToString(),
                SettlementAsset = settlementAsset.DisplayId,
                SettlementAssetAccuracy = settlementAsset.Accuracy,
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

            return Json(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(CreateInvoiceModel model, IFormFileCollection files)
        {
            var result = new InvoiceModel();

            return Json(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteInvoice(string invoiceId)
        {
            await _payInvoiceClient.DeleteInvoiceAsync(MerchantId, invoiceId);
            return NoContent();
        }

        [Route("{invoiceId}/files/{fileId}")]
        public async Task<IActionResult> GetFileSync(string invoiceId, string fileId)
        {
            IEnumerable<FileInfoModel> files = await _payInvoiceClient.GetFilesAsync(invoiceId);
            byte[] content = await _payInvoiceClient.GetFileAsync(invoiceId, fileId);

            FileInfoModel file = files.FirstOrDefault(o => o.Id == fileId);

            return File(content, file.Type, file.Name);
        }
    }
}
