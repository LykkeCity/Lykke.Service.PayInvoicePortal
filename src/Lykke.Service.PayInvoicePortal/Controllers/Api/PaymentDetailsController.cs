using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Route("/api/checkout")]
    public class PaymentDetailsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILog _log;

        public PaymentDetailsController(
            IAssetService assetService,
            IInvoiceService invoiceService,
            ILogFactory logFactory)
        {
            _assetService = assetService;
            _invoiceService = invoiceService;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [Route("{InvoiceId}")]
        [ValidateModel]
        public async Task<IActionResult> Details([Guid] string invoiceId)
        {
            var result = await GetPaymentDetailsAsync(invoiceId, force: false);
            
            return Json(result);
        }

        [HttpPost]
        [Route("refresh/{InvoiceId}")]
        [ValidateModel]
        public async Task<IActionResult> RefreshDetails([Guid] string invoiceId)
        {
            var result = await GetPaymentDetailsAsync(invoiceId, force: true);

            return Json(result);
        }

        [HttpGet]
        [Route("{InvoiceId}/status")]
        [ValidateModel]
        public async Task<IActionResult> Status([Guid] string invoiceId)
        {
            InvoiceStatusModel model = await _invoiceService.GetStatusAsync(invoiceId);
            
            return Json(model);
        }

        [HttpPost]
        [Route("changeasset/{invoiceId}/{paymentAssetId}")]
        [ValidateModel]
        public async Task<IActionResult> ChangePaymentAssetAsync([Guid] string invoiceId, [Required] string paymentAssetId)
        {
            try
            {
                await _invoiceService.ChangePaymentAssetAsync(invoiceId, paymentAssetId);

                var result = await GetPaymentDetailsAsync(invoiceId, force: false);

                return Json(result);
            }
            catch (InvalidOperationException ex)
            {
                _log.ErrorWithDetails(ex, new { invoiceId, paymentAssetId });

                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get payment details from Invoice, PaymentRequest and Order
        /// </summary>
        /// <param name="invoiceId">Invoice id</param>
        /// <param name="force">Will force to create new order if the actual order is expired but can be considered
        ///     as actual till extended due date</param>
        private async Task<PaymentDetailsModel> GetPaymentDetailsAsync(string invoiceId, bool force)
        {
            PaymentDetails paymentDetails = await _invoiceService.GetPaymentDetailsAsync(invoiceId, force);

            if (paymentDetails == null)
            {
                return null;
            }

            var model = Mapper.Map<PaymentDetailsModel>(paymentDetails);

            model.PaymentAssetNetwork = await _assetService.GetAssetNetworkAsync(model.PaymentAssetId);

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

