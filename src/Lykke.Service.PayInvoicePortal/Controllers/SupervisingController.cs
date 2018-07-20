using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Models.Home;
using Microsoft.AspNetCore.Authorization;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    public class SupervisingController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public SupervisingController(
            IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new HomeViewModel();

            return View(vm);
        }
    }
}
