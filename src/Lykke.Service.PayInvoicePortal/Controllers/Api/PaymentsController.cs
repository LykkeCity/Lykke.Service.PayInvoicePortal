using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
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
            ILog log)
        {
            _invoiceService = invoiceService;
            _log = log.CreateComponentScope(nameof(PaymentsController));
        }

        [HttpGet]
        [Route("{InvoiceId}")]
        public async Task<IActionResult> Details(string invoiceId)
        {
            PaymentDetails paymentDetails = await _invoiceService.GetPaymentDetailsAsync(invoiceId, force: true);

            if (paymentDetails == null)
            {
                return Json(null);
            }

            IReadOnlyList<FileInfoModel> files = await _invoiceService.GetFilesAsync(invoiceId);

            var model = Mapper.Map<PaymentDetailsModel>(paymentDetails);
            model.Files = Mapper.Map<List<FileModel>>(files);

            return Json(model);
        }

        [HttpGet]
        [Route("{InvoiceId}/status")]
        public async Task<IActionResult> Status(string invoiceId)
        {
            InvoiceStatus status = await _invoiceService.GetStatusAsync(invoiceId);

            return Json(new
            {
                status = status.ToString()
            });
        }
    }
}
