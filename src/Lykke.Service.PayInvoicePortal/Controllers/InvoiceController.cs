using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoicePortal.Models;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("invoice")]
    public class InvoiceController : BaseController
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ILog _log;

        public InvoiceController(
            IPayInvoiceClient payInvoiceClient,
            IAssetsServiceWithCache assetsService,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _assetsService = assetsService;
            _log = log;
        }

       
        [HttpGet]
        [Route("{InvoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            var model = await GetPaymentDetailsAsync(invoiceId, true);

            return View(model);
        }

        [HttpGet]
        [Route("{InvoiceId}/details")]
        public async Task<IActionResult> Details(string invoiceId)
        {
            var model = await GetPaymentDetailsAsync(invoiceId, false);

            return Json(model);
        }

        [HttpGet]
        [Route("{InvoiceId}/status")]
        public async Task<IActionResult> Status(string invoiceId)
        {
            //InvoiceModel invoice;

            //try
            //{
            //    invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);
            //}
            //catch (Exception exception)
            //{
            //    await _log.WriteErrorAsync(nameof(InvoiceController), nameof(Status), invoiceId, exception);
            //    return BadRequest();
            //}
            
            return Json(new
            {
                status = InvoiceStatus.Unpaid.ToString()
            });
        }

        private async Task<PaymentDetailsModel> GetPaymentDetailsAsync(string invoiceId, bool wait)
        {
            InvoiceDetailsModel invoiceDetails;

            try
            {
                invoiceDetails = await _payInvoiceClient.CheckoutInvoiceAsync(invoiceId);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(InvoiceController), nameof(GetPaymentDetailsAsync), invoiceId, exception);
                return null;
            }

            IEnumerable<FileInfoModel> invoiceFiles = await _payInvoiceClient.GetFilesAsync(invoiceId);

            Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoiceDetails.SettlementAssetId);
            Asset paymentAsset = await _assetsService.TryGetAssetAsync(invoiceDetails.PaymentAssetId);

            int totalSeconds = 0;
            int remainingSeconds = 0;
            bool expired = invoiceDetails.DueDate <= DateTime.UtcNow;
            
            if (!expired && invoiceDetails.Status == InvoiceStatus.Unpaid)
            {
                totalSeconds = (int)(invoiceDetails.OrderDueDate - invoiceDetails.OrderCreatedDate).TotalSeconds;
                remainingSeconds = (int)(invoiceDetails.OrderDueDate - DateTime.UtcNow).TotalSeconds;

                if (remainingSeconds > totalSeconds)
                    remainingSeconds = totalSeconds;
            }

            return new PaymentDetailsModel
            {
                Id = invoiceDetails.Id,
                Number = invoiceDetails.Number,
                Status = invoiceDetails.Status.ToString(),
                Merchant = "Budweiser Stag Brewing Company", //todo
                PaymentAmount = (double)invoiceDetails.PaymentAmount,
                SettlementAmount = (double)invoiceDetails.Amount,
                PaymentAsset = paymentAsset.DisplayId,
                SettlementAsset = settlementAsset.DisplayId,
                PaymentAssetAccuracy = paymentAsset.Accuracy,
                SettlementAssetAccuracy = settlementAsset.Accuracy,
                ExchangeRate = 15000,   //todo
                SpreadPercent = 0.014,  //todo
                FeePercent = 0.086,     //todo
                DueDate = invoiceDetails.DueDate,
                Note = null, //todo
                WalletAddress = invoiceDetails.WalletAddress,
                PaymentRequestId = invoiceDetails.PaymentRequestId,
                TotalSeconds = totalSeconds,
                RemainingSeconds = remainingSeconds,
                Expired = expired,
                Files = invoiceFiles
                    .Select(o => new FileModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Size = o.Size
                    })
                    .ToList()
            };
        }
    }
}
