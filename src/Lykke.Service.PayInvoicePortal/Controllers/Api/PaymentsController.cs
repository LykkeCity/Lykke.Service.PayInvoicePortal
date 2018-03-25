using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.DataService;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Route("/api/payments")]
    public class PaymentsController : Controller
    {
        private readonly InvoiceDataService _invoiceDataService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public PaymentsController(
            InvoiceDataService invoiceDataService,
            IPayInvoiceClient payInvoiceClient,
            ILog log)
        {
            _invoiceDataService = invoiceDataService;
            _payInvoiceClient = payInvoiceClient;
            _log = log;
        }

        [HttpGet]
        [Route("{InvoiceId}")]
        public async Task<IActionResult> Details(string invoiceId)
        {
            PaymentDetailsModel model = await _invoiceDataService.GetPaymentDetailsAsync(invoiceId);

            return Json(model);
        }

        [HttpGet]
        [Route("{InvoiceId}/status")]
        public async Task<IActionResult> Status(string invoiceId)
        {
            InvoiceModel invoice;

            try
            {
                invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(InvoiceController), nameof(Status), invoiceId, exception);
                return BadRequest();
            }

            return Json(new
            {
                status = invoice.Status.ToString()
            });
        }
    }
}
