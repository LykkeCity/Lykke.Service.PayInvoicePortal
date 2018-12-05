using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("invoice")]
    public class InvoiceController : Controller
    {
        [HttpGet]
        [Route("{InvoiceId}")]
        public IActionResult Index(string invoiceId)
        {
            return View();
        }
    }
}
