using System;
using System.Globalization;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Lykke.Service.PayInvoicePortal.Models;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    public class InvoiceController : BaseController
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public InvoiceController(
            IPayInvoiceClient payInvoiceClient,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _log = log;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Profile", "Home");
        }

        [Route("invoice/{InvoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            InvoiceDetailsModel invoiceDetails;

            try
            {
                invoiceDetails = await _payInvoiceClient.CheckoutInvoiceAsync(invoiceId);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(InvoiceController), nameof(Index), invoiceId, exception);
                return NotFound();
            }

            int refreshTime = (int)Math.Round((invoiceDetails.OrderDueDate - DateTime.UtcNow).TotalSeconds);

            decimal amount = Math.Round(invoiceDetails.PaymentAmount, 8);

            var model = new InvoiceViewModel
            {
                InvoiceId = invoiceDetails.Id,
                InvoiceNumber = invoiceDetails.Number,
                Currency = invoiceDetails.SettlementAssetId,
                OrigAmount = TrimDouble(invoiceDetails.Amount, 2),
                ClientName = invoiceDetails.ClientName,
                Amount = TrimDouble(amount, 8),
                PaymentAssetId = invoiceDetails.PaymentAssetId,
                Status = invoiceDetails.Status,
                RefreshSeconds = refreshTime,
                QRCode = $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:{invoiceDetails.WalletAddress}?amount={amount}%26label=invoice%20#{invoiceDetails.Number}%26message={invoiceDetails.PaymentRequestId}",
                AutoUpdate = invoiceDetails.Status == InvoiceStatus.InProgress || invoiceDetails.Status == InvoiceStatus.Unpaid,
                WalletAddress = invoiceDetails.WalletAddress
            };

            return View(model);
        }

        public string TrimDouble(decimal value, int maxSigns)
        {
            var result = value.ToString($"F{maxSigns}", CultureInfo.InvariantCulture);
            result = result.TrimEnd("0".ToCharArray());
            result = result.TrimEnd(".".ToCharArray());
            return result;
        }

        [HttpPost("invoice/status")]
        public async Task<IActionResult> Status(string invoiceId)
        {
            InvoiceModel invoice;

            try
            {
                invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(InvoiceController), nameof(Status), invoiceId, exception);
                return BadRequest();
            }

            return Json(new
            {
                status = invoice.Status.ToString()
            });
        }

        [HttpPost("invoice/update")]
        public IActionResult Update(string invoiceId, string paymentAssetId)
        {
            var rand = new Random();
            
            return Json(new { PaymentAmount = Math.Round(rand.NextDouble(), 8) });
        }
    }
}
