using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInvoicePortal.DataService;
using Lykke.Service.PayInvoicePortal.Models.Email;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/email")]
    public class EmailController : BaseController
    {
        private readonly InvoiceDataService _invoiceDataService;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IEmailPartnerRouterClient _emailPartnerRouterClient;
        private readonly ILog _log;

        public EmailController(
            InvoiceDataService invoiceDataService,
            IPayInternalClient payInternalClient,
            IEmailPartnerRouterClient emailPartnerRouterClient,
            ILog log)
        {
            _invoiceDataService = invoiceDataService;
            _payInternalClient = payInternalClient;
            _emailPartnerRouterClient = emailPartnerRouterClient;
            _log = log;
        }

        [HttpPost]
        public async Task<IActionResult> SendAsync([FromBody] EmailSendModel model)
        {
            InvoiceModel invoice = await _invoiceDataService.GetById(model.InvoiceId);

            MerchantModel merchant = await _payInternalClient.GetMerchantByIdAsync(MerchantId);

            var payload = new Dictionary<string, string>
            {
                {"InvoiceNumber", invoice.Number},
                {"Company", merchant.Name},
                {"ClientFullName", invoice.ClientName},
                {"AmountToBePaid", invoice.Amount.ToString($"N{invoice.SettlementAssetAccuracy}")},
                {"SettlementCurrency", invoice.SettlementAsset},
                {"DueDate", invoice.DueDate.ToString("d")},
                {"CheckoutLink", model.CheckoutUrl},
                {"Note", invoice.Note},
                {"Year", DateTime.Today.Year.ToString()}
            };

            await _emailPartnerRouterClient.Send(new SendEmailCommand
            {
                EmailAddresses = model.Emails.ToArray(),
                Template = "PaymentRequestedTemplate",
                Payload = payload
            });

            return NoContent();
        }
    }
}
