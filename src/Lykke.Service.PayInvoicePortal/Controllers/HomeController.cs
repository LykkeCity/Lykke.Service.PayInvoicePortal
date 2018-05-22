using System.Collections.Generic;
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
            InvoiceSource source = await _invoiceService.GetAsync(
                User.GetMerchantId(),
                new List<InvoiceStatus>(),
                Period.AllTime,
                null,
                null,
                false,
                0,
                20);

            var invoicesGatheredInfo = new InvoicesGatheredInfoModel
            {
                List = new ListModel
                {
                    Total = source.Total,
                    CountPerStatus = source.CountPerStatus.ToDictionary(o => o.Key.ToString(), o => o.Value),
                    Items = Mapper.Map<List<ListItemModel>>(source.Items)
                },
                Balance = source.Balance,
                BaseAsset = source.BaseAsset,
                BaseAssetAccuracy = source.BaseAssetAccuracy,
                Statistic = new StatisticModel
                {
                    MainStatistic = source.MainStatistic.ToDictionary(x => x.Key.ToString(), x => x.Value),
                    SummaryStatistic = source.SummaryStatistic,
                    Rates = source.Rates,
                    HasErrorsInStatistic = source.HasErrorsInStatistic
                }
            };

            var vm = new HomeViewModel
            {
                InvoicesGatheredInfo = invoicesGatheredInfo
            };

            return View(vm);
        }
    }
}
