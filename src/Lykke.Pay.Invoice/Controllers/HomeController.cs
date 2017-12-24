using System.Collections.Generic;
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
using Lykke.Pay.Invoice.AppCode;

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
            var response = await httpClient.PostAsync(MerchantAuthService, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
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
        public async Task<IActionResult> InvoiceDetail(string InvoiceId)
        {
            var model = new InvoiceDetailModel();
            var result = await _invoiceService.ApiInvoicesByInvoiceIdGetWithHttpMessagesAsync(InvoiceId);
            model.Data = result.Body;
            model.InvoiceUrl = Request.Scheme + "://" + Request.Host + "/invoice/" + InvoiceId;
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
                DeleteInvoiceFromBase(model.Data.InvoiceId);
                return Redirect("/home/profile");
            }
            request.Amount = model.Data.Amount.ToString();
            request.ClientEmail = model.Data.ClientEmail;
            request.ClientName = model.Data.ClientName;
            request.Currency = model.Data.Currency;
            request.DueDate = model.Data.DueDate;
            request.InvoiceId = model.Data.InvoiceId;
            request.InvoiceNumber = model.Data.InvoiceNumber;
            request.StartDate = model.Data.StartDate;
            request.WalletAddress = model.Data.WalletAddress;
            request.Status = model.Data.Status;
            await _invoiceService.ApiInvoicesPostWithHttpMessagesAsync(request.CreateEntity());
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
        public async Task<IActionResult> Profile()
        {
            return View();
        }
        [Authorize]
        [HttpPost("profile")]
        public async Task<IActionResult> Profile(Models.InvoiceRequest request, string returnUrl)
        {
            if (string.IsNullOrEmpty(request.InvoiceNumber) || string.IsNullOrEmpty(request.ClientEmail) || string.IsNullOrEmpty(request.Amount.ToString()))
            {
                return View();
            }
            var item = request.CreateEntity();
            var result = await _invoiceService.ApiInvoicesPostWithHttpMessagesAsync(item);
            ViewBag.GeneratedItem = JsonConvert.SerializeObject(item);
            return View();
        }

        [Authorize]
        [HttpPost("balance")]
        public async Task<JsonResult> Balance()
        {
            double amount = 1000;
            return Json($"{string.Format("{0:0.00}", amount)} CHF"); 
        }

        [Authorize]
        [HttpPost("invoices")]
        public async Task<JsonResult> Invoices(GridModel model)
        {
            var respmodel = new GridModel();
            var result = await _invoiceService.ApiInvoicesGetWithHttpMessagesAsync();
            var orderedlist = result.Body.Where(i=>i.Status != InvoiceStatus.Removed.ToString()).OrderByDescending(i => i.StartDate).ToList();
            if (!string.IsNullOrEmpty(model.SearchValue))
            {
                orderedlist = orderedlist.Where(i => (i.WalletAddress != null && i.WalletAddress.Contains(model.SearchValue)) ||
                (i.Currency != null && i.Currency.Contains(model.SearchValue)) ||
                (i.ClientEmail != null && i.ClientEmail.Contains(model.SearchValue)) ||
                (i.ClientName != null && i.ClientName.Contains(model.SearchValue)) ||
                (i.InvoiceNumber != null && i.InvoiceNumber.Contains(model.SearchValue)))
                .OrderByDescending(i => i.StartDate).ToList();
            }
            respmodel.AllCount = orderedlist.Count;
            respmodel.DraftCount = orderedlist.Count(i => i.Status == InvoiceStatus.Draft.ToString());
            respmodel.PaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.Paid.ToString());
            respmodel.UnpaidCount = orderedlist.Count(i=>i.Status == InvoiceStatus.Unpaid.ToString());
            if (!string.IsNullOrEmpty(model.SortField))
            {
                switch(model.SortField)
                {
                    case "number":
                        if (model.SortWay == 0)
                            orderedlist = orderedlist.OrderBy(i => i.InvoiceNumber).OrderByDescending(i => i.StartDate).ToList();
                        else orderedlist = orderedlist.OrderByDescending(i => i.InvoiceNumber).OrderByDescending(i => i.StartDate).ToList();
                        break;
                    case "client":
                        if (model.SortWay == 0)
                            orderedlist = orderedlist.OrderBy(i => i.ClientName).OrderByDescending(i => i.StartDate).ToList();
                        else orderedlist = orderedlist.OrderByDescending(i => i.ClientName).OrderByDescending(i => i.StartDate).ToList();
                        break;
                    case "amount":
                        if (model.SortWay == 0)
                            orderedlist = orderedlist.OrderBy(i => i.Amount).OrderByDescending(i => i.StartDate).ToList();
                        else orderedlist = orderedlist.OrderByDescending(i => i.Amount).OrderByDescending(i => i.StartDate).ToList();
                        break;
                    case "currency":
                        if (model.SortWay == 0)
                            orderedlist = orderedlist.OrderBy(i => i.Currency).OrderByDescending(i => i.StartDate).ToList();
                        else orderedlist = orderedlist.OrderByDescending(i => i.Currency).OrderByDescending(i => i.StartDate).ToList();
                        break;
                    case "status":
                        if (model.SortWay == 0)
                            orderedlist = orderedlist.OrderBy(i => i.Status).OrderByDescending(i => i.StartDate).ToList();
                        else orderedlist = orderedlist.OrderByDescending(i => i.Status).OrderByDescending(i => i.StartDate).ToList();
                        break;
                }
            }
            respmodel.PageCount = orderedlist.Count / 5;
            respmodel.Data = orderedlist.ToPagedList(model.Page, 5).ToList();
            return Json(respmodel);

        }
        [Authorize]
        [HttpGet("deleteinvoice")]
        public async Task<EmptyResult> DeleteInvoice(string invoiceId)
        {
            //_invoiceService.ApiInvoicesByInvoiceIdDeleteGet(invoiceId);
            DeleteInvoiceFromBase(invoiceId);
            return new EmptyResult();
        }
        [HttpGet("SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(HomeUrl);
        }
        protected async void DeleteInvoiceFromBase(string invoiceId)
        {
            _invoiceService.ApiInvoicesByInvoiceIdDeleteGet(invoiceId);
        }
    }
}