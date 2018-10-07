using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    public class SupervisingController : Controller
    {
        public SupervisingController()
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
