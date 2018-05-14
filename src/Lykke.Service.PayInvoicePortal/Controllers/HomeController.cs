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
using Lykke.Service.PayInvoicePortal.Models.Home;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                20);

            Balance balance = await _balanceService.GetAsync(User.GetMerchantId());

            var invoicesGatheredInfo = new InvoicesGatheredInfoModel
            {
                List = new ListModel
                {
                    Total = source.Total,
                    CountPerStatus = source.CountPerStatus.ToDictionary(o => o.Key.ToString(), o => o.Value),
                    Items = Mapper.Map<List<ListItemModel>>(source.Items)
                },
                BaseAsset = source.BaseAsset,
                BaseAssetAccuracy = source.BaseAssetAccuracy,
                Statistic = new StatisticModel
                {
                    MainStatistic = source.Statistic.ToDictionary(x => x.Key.ToString(), x => x.Value),
                    Rates = source.Rates,
                    HasErrorsInStatistic = source.HasErrorsInStatistic
                }
            };

            var vm = new HomeViewModel
            {
                InvoicesGatheredInfo = invoicesGatheredInfo,
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
