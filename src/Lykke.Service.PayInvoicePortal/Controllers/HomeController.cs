using Lykke.Service.PayInvoicePortal.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    [Route("payments")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new HomeViewModel();

            return View(vm);
        }
    }
}
