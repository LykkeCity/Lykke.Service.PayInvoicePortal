using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.DataService;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    [Route("/invoices")]
    public class InvoicesController : BaseController
    {
        private readonly InvoiceDataService _invoiceDataService;

        public InvoicesController(InvoiceDataService invoiceDataService)
        {
            _invoiceDataService = invoiceDataService;
        }

        [HttpGet]
        [Route("{invoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            InvoiceModel invoice = await _invoiceDataService.GetById(invoiceId);

            var vm = new IndexViewModel
            {
                Invoice = invoice,
                BlockchainExplorerUrl = $"{BlockchainExplorerUrl.TrimEnd('/')}/address/{invoice.WalletAddress}"
            };

            return View(vm);
        }
    }
}
