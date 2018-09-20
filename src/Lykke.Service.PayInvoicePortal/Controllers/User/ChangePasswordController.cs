using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.User
{
    [Authorize]
    [Route("settings/changePassword")]
    public class ChangePasswordController : Controller
    {
        [HttpGet]
        
        public IActionResult ChangePassword()
        {
            return View("../User/ChangePassword");
        }
    }
}
