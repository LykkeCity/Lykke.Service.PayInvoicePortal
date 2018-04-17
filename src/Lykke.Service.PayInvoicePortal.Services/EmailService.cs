using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Services;
using MoreLinq;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class EmailService : IEmailService
    {
        private const string EmailTemplate = "PaymentRequestedTemplate";
        private const string EmailTemplateWithoutNote = "PaymentRequestedWithoutNoteTemplate";

        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly IEmailPartnerRouterClient _emailPartnerRouterClient;
        private readonly ILog _log;

        public EmailService(
            IPayInvoiceClient payInvoiceClient,
            IPayInternalClient payInternalClient,
            IAssetsServiceWithCache assetsService,
            IEmailPartnerRouterClient emailPartnerRouterClient,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _payInternalClient = payInternalClient;
            _assetsService = assetsService;
            _emailPartnerRouterClient = emailPartnerRouterClient;
            _log = log;
        }

        public async Task<bool> SendAsync(string invoiceId, string checkoutUrl, IReadOnlyList<string> addresses)
        {
            InvoiceModel invoice;
            MerchantModel merchant;

            try
            {
                invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);
                merchant = await _payInternalClient.GetMerchantByIdAsync(invoice.MerchantId);
            }
            catch (Exception exception)
            {
                _log.WriteError(nameof(EmailService), new {invoiceId}, exception);
                return false;
            }

            Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoice.SettlementAssetId);

            var payload = new Dictionary<string, string>
            {
                {"InvoiceNumber", invoice.Number},
                {"Company", merchant.DisplayName},
                {"ClientFullName", invoice.ClientName},
                {"AmountToBePaid", invoice.Amount.ToString($"N{settlementAsset.Accuracy}")},
                {"SettlementCurrency", settlementAsset.DisplayId},
                {"DueDate", invoice.DueDate.ToString("d")},
                {"CheckoutLink", checkoutUrl},
                {"Note", invoice.Note},
                {"Year", DateTime.Today.Year.ToString()}
            };

            string template = string.IsNullOrEmpty(invoice.Note) ? EmailTemplateWithoutNote : EmailTemplate;

            try
            {
                await _emailPartnerRouterClient.Send(new SendEmailCommand
                {
                    EmailAddresses = addresses.ToArray(),
                    Template = template,
                    Payload = payload
                });
            }
            catch (Exception exception)
            {
                _log.WriteError(nameof(EmailService), new { invoiceId, template }, exception);
                return false;
            }

            return true;
        }
    }
}
