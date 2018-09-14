using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.User
{
    [Authorize]
    [Route("settings")]
    public class SettingsController : Controller
    {
        [HttpGet]
        
        public IActionResult Settings()
        {
            return View("../User/Settings");
        }
    }
}
