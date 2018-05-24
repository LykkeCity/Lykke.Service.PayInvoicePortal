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
        private readonly ILog _log;

        public SupervisingController(
            IInvoiceService invoiceService,
            ILog log)
        {
            _invoiceService = invoiceService;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel();

            return View(vm);
        }
    }
}
