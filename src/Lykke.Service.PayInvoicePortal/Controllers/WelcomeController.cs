using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    public class WelcomeController : Controller
    {
        [HttpGet]
        public IActionResult Welcome()
        {
            return View();
        }
    }
}
