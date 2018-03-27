using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Models.Auth;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("welcome")]
    public class AuthController : Controller
    {
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public AuthController(
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient,
            ILog log)
        {
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
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

            ValidateResultModel response;

            try
            {
                response = await _payAuthClient.ValidateAsync(model.Login, model.Password);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(AuthController), nameof(SignIn),
                    $"Can not authenticate user '{model.Login}'", exception);

                ModelState.AddModelError(string.Empty, "An error occurred during authentication.");
                return View(vm);
            }

            if (!response.Success)
            {
                ModelState.AddModelError(string.Empty, "The e-mail or password you entered incorrect.");
                return View(vm);
            }

            EmployeeModel employee;

            try
            {
                employee = await _payInvoiceClient.GetEmployeeAsync(response.MerchantId, response.EmployeeId);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(AuthController), nameof(SignIn),
                    $"Can not get employee. {nameof(response.MerchantId)}: '{response.MerchantId}'. {nameof(response.EmployeeId)}: '{response.EmployeeId}'",
                    exception);

                ModelState.AddModelError(string.Empty, "User profile not found.");
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
