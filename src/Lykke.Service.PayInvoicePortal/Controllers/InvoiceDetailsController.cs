using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    [Route("/invoices")]
    public class InvoiceDetailsController : Controller
    {
        [HttpGet]
        [Route("{invoiceId}")]
        public async Task<IActionResult> InvoiceDetails(string invoiceId)
        {
            return View();
        }
    }
}
