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
using Lykke.Service.PayInvoicePortal.DataService;
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

        private readonly InvoiceDataService _invoiceDataService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IInvoiceService _invoiceService;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ILog _log;

        public InvoicesController(
            InvoiceDataService invoiceDataService,
            IPayInvoiceClient payInvoiceClient,
            IInvoiceService invoiceService,
            IAssetsServiceWithCache assetsService,
            ILog log)
        {
            _invoiceDataService = invoiceDataService;
            _payInvoiceClient = payInvoiceClient;
            _invoiceService = invoiceService;
            _assetsService = assetsService;
            _log = log;
        }

        [HttpGet]
        [Route("{invoiceId}")]
        public async Task<IActionResult> GetByIdAsync(string invoiceId)
        {
            InvoiceModel result = await _invoiceDataService.GetById(invoiceId);

            return Json(result);
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
            ListModel result = await _invoiceDataService.GetListSourceAsync(
                MerchantId,
                searchValue,
                period,
                status,
                sortField,
                sortAscending,
                skip,
                take);
            
            return Json(result);
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
                    DueDate = model.DueDate,
                    Note = model.Note
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
                    DueDate = model.DueDate,
                    Note = model.Note
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

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> UpdateAsync(UpdateInvoiceModel model, IFormFileCollection files)
        {
            if (model.IsDraft)
            {
                await _payInvoiceClient.UpdateDraftInvoiceAsync(MerchantId, model.Id, new PayInvoice.Client.Models.Invoice.CreateDraftInvoiceModel
                {
                    EmployeeId = EmployeeId,
                    Number = model.Number,
                    ClientName = model.Client,
                    ClientEmail = model.Email,
                    Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                    SettlementAssetId = model.SettlementAsset,
                    PaymentAssetId = PaymentAssetId,
                    DueDate = model.DueDate,
                    Note = model.Note
                });
            }
            else
            {
                await _payInvoiceClient.CreateInvoiceFromDraftAsync(MerchantId, model.Id, new PayInvoice.Client.Models.Invoice.CreateInvoiceModel
                {
                    EmployeeId = EmployeeId,
                    Number = model.Number,
                    ClientName = model.Client,
                    ClientEmail = model.Email,
                    Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                    SettlementAssetId = model.SettlementAsset,
                    PaymentAssetId = PaymentAssetId,
                    DueDate = model.DueDate,
                    Note = model.Note
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

                    await _payInvoiceClient.UploadFileAsync(model.Id, content, formFile.FileName, formFile.ContentType);
                }
            }

            return NoContent();
        }

        [HttpDelete]
        [Route("{invoiceId}")]
        public async Task<IActionResult> DeleteInvoice(string invoiceId)
        {
            await _payInvoiceClient.DeleteInvoiceAsync(MerchantId, invoiceId);
            return NoContent();
        }
    }
}
