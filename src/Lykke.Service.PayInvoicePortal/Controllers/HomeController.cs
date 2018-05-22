﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models.Home;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Lykke.Service.PayInvoicePortal.Models.Invoices.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILog _log;

        public HomeController(
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
