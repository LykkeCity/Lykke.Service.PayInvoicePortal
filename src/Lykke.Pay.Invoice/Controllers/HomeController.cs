using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Lykke.Pay.Common.Entities.Entities;
using Lykke.Pay.Invoice.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Lykke.Pay.Service.Invoces.Client;
using System.Linq;
using Lykke.Pay.Common;
using PagedList;

namespace Lykke.Pay.Invoice.Controllers
{
    [Route("home")]
    public class HomeController : BaseController
    {
        private readonly IInvoicesservice _invoiceService;

        public HomeController(IConfiguration configuration, IInvoicesservice invoiceService) : base(configuration)
        {
            _invoiceService = invoiceService;
        }



        [HttpGet("welcome")]
        public IActionResult Welcome(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost("welcome")]
        public async Task<IActionResult> Welcome(MerchantStaffSignInRequest request, string returnUrl)
        {
            //var request = new MerchantStaffSignInRequest
            //{
            //    Login = login,
            //    Password = password
            //};
            if (string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
            {
                ViewBag.Error = true;
                return View();
            }
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync($"{MerchantAuthService}staffSignIn", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var rstText = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(rstText))
            {
                ViewBag.Error = true;
                return View();
            }


            var user = JsonConvert.DeserializeObject<MerchantStaff>(rstText);

            if (user == null)
            {
                ViewBag.Error = true;
                return View();
            }


            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Sid, user.MerchantStaffEmail),
                    new Claim(ClaimTypes.UserData, rstText),
                    new Claim(ClaimTypes.Name, $"{user.MerchantStaffFirstName} {user.MerchantStaffLastName}")
                };

            var claimsIdentity = new ClaimsIdentity(claims, "password");
            var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrinciple);

            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : HomeUrl);

        }

        [Authorize]
        [HttpGet("InvoiceDetail")]
        public async Task<IActionResult> InvoiceDetail(string invoiceId)
        {
            var model = new InvoiceDetailModel();
            var result = await _invoiceService.ApiInvoicesByInvoiceIdGetWithHttpMessagesAsync(invoiceId);
            model.Data = result.Body;
            model.InvoiceUrl = Request.Scheme + "://" + Request.Host + "/invoice/" + invoiceId;
            if (model.Data.Status != InvoiceStatus.Paid.ToString())
                model.QRCode =
                    $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:{model.Data.WalletAddress}?amount={model.Data.Amount}%26label=LykkePay%26message={model.Data.InvoiceId}";
            return View(model);
        }
        [Authorize]
        [HttpPost("InvoiceDetail")]
        public async Task<IActionResult> InvoiceDetail(InvoiceDetailModel model)
        {
            var request = new Models.InvoiceRequest();
            if(model.Data.Status == InvoiceStatus.Removed.ToString())
            {
                await DeleteInvoiceFromBase(model.Data.InvoiceId);
                return Redirect("/home/profile");
            }
            request.Amount = model.Data.Amount.ToString(CultureInfo.InvariantCulture);
            request.ClientEmail = model.Data.ClientEmail;
            request.ClientName = model.Data.ClientName;
            request.Currency = model.Data.Currency;
            request.DueDate = model.Data.DueDate;
            request.InvoiceId = model.Data.InvoiceId;
            request.InvoiceNumber = model.Data.InvoiceNumber;
            request.StartDate = model.Data.StartDate;
            request.WalletAddress = model.Data.WalletAddress;
            request.Status = model.Data.Status;
            await _invoiceService.ApiInvoicesPostWithHttpMessagesAsync(request.CreateEntity(OrderLiveTime));
            if (model.Data.Status != InvoiceStatus.Paid.ToString())
            {
                model.QRCode =
                    $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:{model.Data.WalletAddress}?amount={model.Data.Amount}%26label=LykkePay%26message={model.Data.InvoiceId}";
                model.InvoiceUrl = Request.Scheme + "://" + Request.Host + "/invoice/" + model.Data.InvoiceId;
            }
            return View(model);
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return View();
        }
        [Authorize]
        [HttpPost("profile")]
        public async Task<IActionResult> Profile(Models.InvoiceRequest request, string returnUrl)
        {
            if (request.Status != InvoiceStatus.Draft.ToString() && (string.IsNullOrEmpty(request.InvoiceNumber) || string.IsNullOrEmpty(request.ClientEmail) || string.IsNullOrEmpty(request.Amount)))
            {
                return View();
            }
            var item = request.CreateEntity(OrderLiveTime);
            await _invoiceService.ApiInvoicesPostWithHttpMessagesAsync(item);
            ViewBag.GeneratedItem = JsonConvert.SerializeObject(item);
            return View();
        }

        [Authorize]
        [HttpPost("balance")]
        public async Task<JsonResult> Balance()
        {
            var assert = "CHF";
            
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{MerchantAuthService}balance/{User.Claims.First(u => u.Type == ClaimTypes.Sid).Value}");
            var rstText = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(rstText))
            {
               
                return Json($"0.00 {assert}");
            }

            var balance = JsonConvert.DeserializeObject<MerchantBalance>(rstText);
            if (balance == null)
            {
                return Json($"0.00 {assert}");
            }

            var assertBalance =
                balance.Asserts.FirstOrDefault(
                    ab => ab.Assert.Equals(assert, StringComparison.InvariantCultureIgnoreCase));

            if (assertBalance == null)
            {
                return Json($"0.00 {assert}");
            }

            return Json($"{string.Format("{0:0.00}", assertBalance.Assert)}  {assert}"); 
        }

        [Authorize]
        [HttpPost("invoices")]
        public async Task<JsonResult> Invoices(GridModel model)
        {
            var respmodel = new GridModel();
            var result = await _invoiceService.ApiInvoicesGetWithHttpMessagesAsync();
            var orderedlist = result.Body.OrderByDescending(i => i.StartDate).ToList();
            if (model.Filter.Status != "All")
            {
                orderedlist = orderedlist.Where(i => i.Status == model.Filter.Status).ToList();
            }
            if (!string.IsNullOrEmpty(model.Filter.SearchValue))
            {
                orderedlist = orderedlist.Where(i => (i.WalletAddress != null && i.WalletAddress.Contains(model.Filter.SearchValue)) ||
                (i.Currency != null && i.Currency.Contains(model.Filter.SearchValue)) ||
                (i.ClientEmail != null && i.ClientEmail.Contains(model.Filter.SearchValue)) ||
                (i.ClientName != null && i.ClientName.Contains(model.Filter.SearchValue)) ||
                (i.InvoiceNumber != null && i.InvoiceNumber.Contains(model.Filter.SearchValue)))
                .OrderByDescending(i => i.StartDate).ToList();
            }
            if (model.Filter.Status == "All")
            {
                respmodel.Header.AllCount = orderedlist.Count;
                respmodel.Header.DraftCount = orderedlist.Count(i => i.Status == InvoiceStatus.Draft.ToString());
                respmodel.Header.PaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.Paid.ToString());
                respmodel.Header.UnpaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.Unpaid.ToString());
                respmodel.Header.RemovedCount = orderedlist.Count(i => i.Status == InvoiceStatus.Removed.ToString());
                respmodel.Header.InProgressCount =
                    orderedlist.Count(i => i.Status == InvoiceStatus.InProgress.ToString());
                respmodel.Header.LatePaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.LatePaid.ToString());
                respmodel.Header.UnderpaidCount =
                    orderedlist.Count(i => i.Status == InvoiceStatus.Underpaid.ToString());
                respmodel.Header.OverpaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.Overpaid.ToString());
            }
            respmodel.Filter.Status = model.Filter.Status;
            if (!string.IsNullOrEmpty(model.Filter.SortField))
            {
                switch(model.Filter.SortField)
                {
                    case "number":
                        orderedlist = model.Filter.SortWay == 0 ? orderedlist.OrderBy(i => i.InvoiceNumber).ThenByDescending(i => i.StartDate).ToList() : orderedlist.OrderByDescending(i => i.InvoiceNumber).ThenByDescending(i => i.StartDate).ToList();
                        break;
                    case "client":
                        orderedlist = model.Filter.SortWay == 0 ? orderedlist.OrderBy(i => i.ClientName).ThenByDescending(i => i.StartDate).ToList() : orderedlist.OrderByDescending(i => i.ClientName).ThenByDescending(i => i.StartDate).ToList();
                        break;
                    case "amount":
                        orderedlist = model.Filter.SortWay == 0 ? orderedlist.OrderBy(i => i.Amount).ThenByDescending(i => i.StartDate).ToList() : orderedlist.OrderByDescending(i => i.Amount).ThenByDescending(i => i.StartDate).ToList();
                        break;
                    case "currency":
                        orderedlist = model.Filter.SortWay == 0 ? orderedlist.OrderBy(i => i.Currency).ThenByDescending(i => i.StartDate).ToList() : orderedlist.OrderByDescending(i => i.Currency).ThenByDescending(i => i.StartDate).ToList();
                        break;
                    case "status":
                        orderedlist = model.Filter.SortWay == 0 ? orderedlist.OrderBy(i => i.Status).ThenByDescending(i => i.StartDate).ToList() : orderedlist.OrderByDescending(i => i.Status).ThenByDescending(i => i.StartDate).ToList();
                        break;
                }
            }
            respmodel.PageCount = orderedlist.Count / 20;
            respmodel.Data = orderedlist.ToPagedList(model.Page, 20).ToList();
            return Json(respmodel);

        }
        [Authorize]
        [HttpGet("deleteinvoice")]
        public async Task<EmptyResult> DeleteInvoice(string invoiceId)
        {
            //_invoiceService.ApiInvoicesByInvoiceIdDeleteGet(invoiceId);
            await DeleteInvoiceFromBase(invoiceId);
            return new EmptyResult();
        }
        [HttpGet("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(HomeUrl);
        }
        protected async Task DeleteInvoiceFromBase(string invoiceId)
        {
            await _invoiceService.ApiInvoicesByInvoiceIdDeleteGetWithHttpMessagesAsync(invoiceId);
        }
    }
}