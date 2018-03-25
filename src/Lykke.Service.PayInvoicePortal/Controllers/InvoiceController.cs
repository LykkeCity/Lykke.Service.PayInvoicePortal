using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.PayInvoicePortal.DataService;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("invoice")]
    public class InvoiceController : BaseController
    {
        private readonly InvoiceDataService _invoiceDataService;
        private readonly ILog _log;

        public InvoiceController(
            InvoiceDataService invoiceDataService,
            ILog log)
        {
            _invoiceDataService = invoiceDataService;
            _log = log;
        }
        
        [HttpGet]
        [Route("{InvoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            PaymentDetailsModel model = await _invoiceDataService.GetPaymentDetailsAsync(invoiceId);

            if (model == null)
                return NotFound();

            var vm = new InvoiceViewModel
            {
                PaymentDetails = model
            };

            return View(vm);
        }
    }
}
