using System;
using System.Globalization;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Pay.Common;
using Lykke.Pay.Invoice.AppCode;
using Lykke.Pay.Invoice.Models.Invoice;
using Lykke.Pay.Service.Invoces.Client;
using Lykke.Pay.Service.Invoces.Client.Models.Invoice;
using Microsoft.AspNetCore.Mvc;


namespace Lykke.Pay.Invoice.Controllers
{
    public class InvoiceController : BaseController
    {
        private readonly IPayInvoicesServiceClient _invoicesServiceClient;
        private readonly ILog _log;

        public InvoiceController(
            AppSettings settings,
            IPayInvoicesServiceClient invoicesServiceClient,
            ILog log)
            : base(settings)
        {
            _invoicesServiceClient = invoicesServiceClient;
            _log = log;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Profile", "Home");
        }

        [Route("invoice/{InvoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            InvoiceSummaryModel invoiceSummary;

            try
            {
                invoiceSummary = await _invoicesServiceClient.GetInvoiceSummaryAsync(invoiceId);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(InvoiceController), nameof(Index), invoiceId, exception);
                return NotFound();
            }

            int refreshTime = (int)Math.Round(string.IsNullOrEmpty(invoiceSummary.TransactionTime)
                ? OrderLiveTime.TotalSeconds
                : (invoiceSummary.TransactionTime.FromUnixFormat() - DateTime.Now).TotalSeconds);

            double amoint = Math.Round(invoiceSummary.OrderAmount, 8);

            var model = new InvoiceViewModel
            {
                InvoiceId = invoiceSummary.InvoiceId,
                InvoiceNumber = invoiceSummary.InvoiceNumber,
                Currency = invoiceSummary.InvoiceCurrency,
                OrigAmount = TrimDouble(invoiceSummary.InvoiceAmount, 2),
                ClientName = invoiceSummary.ClientName,
                Amount = TrimDouble(amoint, 8),
                Status = invoiceSummary.Status,
                RefreshSeconds = refreshTime,
                QRCode = $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:{invoiceSummary.WalletAddress}?amount={amoint}%26label=invoice%20#{invoiceSummary.InvoiceNumber}%26message={invoiceSummary.OrderId}",
                AutoUpdate = invoiceSummary.Status == InvoiceStatus.InProgress.ToString() || invoiceSummary.Status == InvoiceStatus.Unpaid.ToString(),
                WalletAddress = invoiceSummary.WalletAddress
            };

            return View(model);
        }

        public string TrimDouble(double value, int maxSigns)
        {
            var result = value.ToString($"F{maxSigns}", CultureInfo.InvariantCulture);
            result = result.TrimEnd("0".ToCharArray());
            result = result.TrimEnd(".".ToCharArray());
            return result;
        }

        [HttpPost("invoice/status")]
        public async Task<IActionResult> Status(string invoiceId)
        {
            InvoiceSummaryModel invoiceSummary;

            try
            {
                invoiceSummary = await _invoicesServiceClient.GetInvoiceSummaryAsync(invoiceId);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(InvoiceController), nameof(Status), invoiceId, exception);
                return BadRequest();
            }

            return Json(new
            {
                status = invoiceSummary.Status
            });
        }
    }
}
