using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    [Route("payments")]
    public class PaymentsController : Controller
    {
        [HttpGet]
        public IActionResult Payments()
        {
            return View();
        }
    }
}
