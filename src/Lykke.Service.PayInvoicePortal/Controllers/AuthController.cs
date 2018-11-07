using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.Models.Auth;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.PayInternal.Client;
using System.Linq;
using Common;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Client.Models.SupervisorMembership;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Controllers.User;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;

        public AuthController(
            IAuthService authService,
            IPayInternalClient payInternalClient,
            ILogFactory logFactory)
        {
            _authService = authService;
            _payInternalClient = payInternalClient;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [Route("signin")]
        public IActionResult SignIn(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
               return RedirectToAction(nameof(PaymentsController.Payments), "Payments");

            return View(new SignInViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            var vm = new SignInViewModel
            {
                ReturnUrl = model.ReturnUrl
            };

            if (!ModelState.IsValid)
                return View(vm);

            _log.Info($"Starting SignIn for {model.Login.Substring(0, 3)}...");

            var result = await _authService.ValidateAsync(model.Login, model.Password);

            var employee = result.Employee;

            if (employee == null || employee.IsDeleted)
            {
                ModelState.AddModelError(string.Empty, "The e-mail or password you entered incorrect.");
                return View(vm);
            }

            if (result.ValidateResult.ForceEmailConfirmation)
            {
                _log.Info($"Login with unconfirmed email, details: {result.ValidateResult.ToJson()}");

                return RedirectToAction(nameof(SignupController.GetStarted), "Signup", new
                {
                    email = employee.Email
                });
            }

            _log.Info("Start GetSupervisorMembershipAsync");

            var sw = Stopwatch.StartNew();

            SupervisorMembershipResponse supervisorMembership =
                await _payInternalClient.GetSupervisorMembershipAsync(employee.Id);

            _log.Info($"Finished GetSupervisorMembershipAsync for {sw.ElapsedMilliseconds} ms");

            bool isSupervisor = supervisorMembership?.MerchantGroups.Any() ?? false;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, employee.Id),
                new Claim(ClaimTypes.UserData, employee.MerchantId),
                new Claim(ClaimTypes.Actor, isSupervisor.ToString()),
                new Claim(ClaimTypes.Email, employee.Email),
                new Claim(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "password");
            var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);

            sw.Restart();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrinciple);

            _log.Info($"Made HttpContext.SignInAsync for {sw.ElapsedMilliseconds} ms");

            _log.Info($"Finished SignIn for {model.Login.Substring(0, 3)}...");

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction(nameof(PaymentsController.Payments), "Payments");
        }

        [HttpGet("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Welcome", "Welcome");
        }
    }
}
