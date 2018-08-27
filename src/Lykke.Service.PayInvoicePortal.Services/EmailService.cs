using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PayMerchant.Client.Models;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Common.Log;
using Lykke.Service.PayMerchant.Client;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class EmailService : IEmailService
    {
        private const string EmailTemplate = "PaymentRequestedTemplate";
        private const string EmailTemplateWithoutNote = "PaymentRequestedWithoutNoteTemplate";

        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly IEmailPartnerRouterClient _emailPartnerRouterClient;
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly ILog _log;

        public EmailService(
            IPayInvoiceClient payInvoiceClient,
            ILykkeAssetsResolver lykkeAssetsResolver,
            IEmailPartnerRouterClient emailPartnerRouterClient,
            ILogFactory logFactory, 
            IPayMerchantClient payMerchantClient)
        {
            _payInvoiceClient = payInvoiceClient;
            _lykkeAssetsResolver = lykkeAssetsResolver;
            _emailPartnerRouterClient = emailPartnerRouterClient;
            _payMerchantClient = payMerchantClient;
            _log = logFactory.CreateLog(this);
        }

        public async Task<bool> SendAsync(string invoiceId, string checkoutUrl, IReadOnlyList<string> addresses)
        {
            InvoiceModel invoice;
            MerchantModel merchant;

            try
            {
                invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);
                merchant = await _payMerchantClient.Api.GetByIdAsync(invoice.MerchantId);
            }
            catch (Exception ex)
            {
                _log.ErrorWithDetails(ex, new { invoiceId });
                return false;
            }

            Asset settlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.SettlementAssetId);

            var payload = new Dictionary<string, string>
            {
                {"InvoiceNumber", invoice.Number},
                {"Company", merchant.DisplayName},
                {"ClientFullName", invoice.ClientName},
                {"AmountToBePaid", invoice.Amount.ToStringNoZeros(settlementAsset.Accuracy)},
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
            catch (Exception ex)
            {
                _log.ErrorWithDetails(ex, new { invoiceId, template });
                return false;
            }

            return true;
        }
    }
}
