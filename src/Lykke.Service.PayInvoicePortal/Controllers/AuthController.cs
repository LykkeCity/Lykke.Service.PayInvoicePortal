using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Models.Auth;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("welcome")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILog _log;

        public AuthController(
            IAuthService authService,
            ILog log)
        {
            _authService = authService;
            _log = log;
        }

        [HttpGet]
        public IActionResult SignIn(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
               return RedirectToAction(nameof(HomeController.Index), "Home");

            return View(new SignInViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            var vm = new SignInViewModel
            {
                ReturnUrl = model.ReturnUrl
            };

            if (!ModelState.IsValid)
                return View(vm);

            EmployeeModel employee = await _authService.ValidateAsync(model.Login, model.Password);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "The e-mail or password you entered incorrect.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, employee.Id),
                new Claim(ClaimTypes.UserData, employee.MerchantId),
                new Claim(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "password");
            var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrinciple);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Auth");
        }
    }
}
