using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/invoices")]
    public class InvoicesController : Controller
    {
        private const string PaymentAssetId = "BTC";

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
        public async Task<IActionResult> GetByIdAsync(string invoiceId)
        {
            var invoiceTask = _invoiceService.GetByIdAsync(invoiceId);
            var filesTask = _invoiceService.GetFilesAsync(invoiceId);
            var historyTask = _invoiceService.GetHistoryAsync(User.GetMerchantId(), invoiceId);

            await Task.WhenAll(invoiceTask, filesTask, historyTask);

            var model = Mapper.Map<InvoiceModel>(invoiceTask.Result);
            model.Files = Mapper.Map<List<FileModel>>(filesTask.Result);
            model.History = Mapper.Map<List<HistoryItemModel>>(historyTask.Result);

            return Json(model);
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
                User.GetMerchantId(),
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
                CountPerStatus = source.CountPerStatus.ToDictionary(o => o.Key.ToString(), o => o.Value),
                Items = Mapper.Map<List<ListItemModel>>(source.Items)
            };
            
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(CreateInvoiceModel model, IFormFileCollection files)
        {
            var request = new PayInvoice.Client.Models.Invoice.CreateInvoiceModel
            {
                MerchantId = User.GetMerchantId(),
                EmployeeId = User.GetEmployeeId(),
                Number = model.Number,
                ClientName = model.Client,
                ClientEmail = model.Email,
                Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                SettlementAssetId = model.SettlementAsset,
                PaymentAssetId = PaymentAssetId,
                DueDate = model.DueDate,
                Note = model.Note
            };

            Invoice invoice = await _invoiceService.CreateAsync(request, model.IsDraft);

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

                    await _invoiceService.UploadFileAsync(invoice.Id, content, formFile.FileName, formFile.ContentType);
                }
            }

            IEnumerable<FileInfoModel> invoiceFiles = await _invoiceService.GetFilesAsync(invoice.Id);

            var result = Mapper.Map<InvoiceModel>(invoice);
            result.Files = Mapper.Map<List<FileModel>>(invoiceFiles);

            return Json(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody]UpdateInvoiceModel model)
        {
            var invoice = new PayInvoice.Client.Models.Invoice.UpdateInvoiceModel
            {
                Id = model.Id,
                MerchantId = User.GetMerchantId(),
                EmployeeId = User.GetEmployeeId(),
                Number = model.Number,
                ClientName = model.Client,
                ClientEmail = model.Email,
                Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                SettlementAssetId = model.SettlementAsset,
                PaymentAssetId = PaymentAssetId,
                DueDate = model.DueDate,
                Note = model.Note
            };

            await _invoiceService.UpdateAsync(invoice, model.IsDraft);
            
            return NoContent();
        }

        [HttpDelete]
        [Route("{invoiceId}")]
        public async Task<IActionResult> DeleteInvoice(string invoiceId)
        {
            await _invoiceService.DeleteAsync(invoiceId);
            return NoContent();
        }
    }
}
