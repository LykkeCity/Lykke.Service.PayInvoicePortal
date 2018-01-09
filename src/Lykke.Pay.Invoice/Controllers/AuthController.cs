using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Common;
using Lykke.Pay.Invoice.Clients.MerchantAuth;
using Lykke.Pay.Invoice.Clients.MerchantAuth.Models;
using Lykke.Pay.Invoice.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Pay.Invoice.Controllers
{
    [Route("welcome")]
    public class AuthController : Controller
    {
        private readonly IMerchantAuthServiceClient _merchantAuthServiceClient;

        public AuthController(IMerchantAuthServiceClient merchantAuthServiceClient)
        {
            _merchantAuthServiceClient = merchantAuthServiceClient;
        }

        [HttpGet]
        public IActionResult SignIn(string returnUrl)
        {
            return View(new SignInViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = true;
                return View();
            }

            MerchantStaffSignInResponse response = await _merchantAuthServiceClient
                .SignInAsync(new MerchantStaffSignInRequest
                {
                    Login = model.Login,
                    Password = model.Password
                });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, response.MerchantStaffEmail),
                new Claim(ClaimTypes.UserData, response.ToJson()),
                new Claim(ClaimTypes.Name, $"{response.MerchantStaffFirstName} {response.MerchantStaffLastName}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "password");
            var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrinciple);

            if(string.IsNullOrEmpty(model.ReturnUrl) || !Url.IsLocalUrl(model.ReturnUrl))
                return RedirectToAction("Profile", "Home");

            return Redirect(model.ReturnUrl);
        }

        [HttpGet("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Profile", "Home");
        }
    }
}
