using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Route("/api/payments")]
    public class PaymentsController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILog _log;

        public PaymentsController(
            IInvoiceService invoiceService,
            ILogFactory logFactory)
        {
            _invoiceService = invoiceService;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [Route("{InvoiceId}")]
        public async Task<IActionResult> Details(string invoiceId)
        {
            var result = await GetPaymentDetailsAsync(invoiceId, force: false);
            
            return Json(result);
        }

        [HttpGet]
        [Route("refresh/{InvoiceId}")]
        public async Task<IActionResult> RefreshDetails(string invoiceId)
        {
            var result = await GetPaymentDetailsAsync(invoiceId, force: true);

            return Json(result);
        }

        [HttpGet]
        [Route("{InvoiceId}/status")]
        public async Task<IActionResult> Status(string invoiceId)
        {
            InvoiceStatusModel model = await _invoiceService.GetStatusAsync(invoiceId);
            
            return Json(model);
        }

        [HttpPost]
        [Route("changeasset/{invoiceId}/{paymentAssetId}")]
        public async Task<IActionResult> ChangePaymentAssetAsync(string invoiceId, string paymentAssetId)
        {
            try
            {
                await _invoiceService.ChangePaymentAssetAsync(invoiceId, paymentAssetId);

                var result = await GetPaymentDetailsAsync(invoiceId, force: false);

                return Json(result);
            }
            catch (InvalidOperationException ex)
            {
                _log.Error(ex, new { invoiceId, paymentAssetId });

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get payment details from Invoice, PaymentRequest and Order
        /// </summary>
        /// <param name="force">Will force to create new order if the actual order is expired but can be considered
        //     as actual till extended due date</param>
        private async Task<PaymentDetailsModel> GetPaymentDetailsAsync(string invoiceId, bool force)
        {
            PaymentDetails paymentDetails = await _invoiceService.GetPaymentDetailsAsync(invoiceId, force);

            if (paymentDetails == null)
            {
                return null;
            }

            var model = Mapper.Map<PaymentDetailsModel>(paymentDetails);
            await UpdateFiles(model);

            return model;
        }

        private async Task UpdateFiles(PaymentDetailsModel model)
        {
            IReadOnlyList<FileInfoModel> files = await _invoiceService.GetFilesAsync(model.Id);
            model.Files = Mapper.Map<List<FileModel>>(files);
        }
    }
}
