using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models.Balances;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndexViewModel = Lykke.Service.PayInvoicePortal.Models.Home.IndexViewModel;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IBalanceService _balanceService;
        private readonly ILog _log;

        public HomeController(
            IInvoiceService invoiceService,
            IBalanceService balanceService,
            ILog log)
        {
            _invoiceService = invoiceService;
            _balanceService = balanceService;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            InvoiceSource source = await _invoiceService.GetAsync(
                User.GetMerchantId(),
                new List<InvoiceStatus>(),
                Period.AllTime,
                null,
                null,
                false,
                0,
                10);

            Balance balance = await _balanceService.GetAsync(User.GetMerchantId());

            var vm = new IndexViewModel
            {
                List = new ListModel
                {
                    Total = source.Total,
                    CountPerStatus = source.CountPerStatus.ToDictionary(o => o.Key.ToString(), o => o.Value),
                    Items = Mapper.Map<List<ListItemModel>>(source.Items)
                },
                Balance = new BalanceModel
                {
                    Value = balance.Value,
                    Currency = balance.Currency
                }
            };

            return View(vm);
        }
    }
}
