using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Services.Extensions;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;

        public PaymentsService(
            IPayInvoiceClient payInvoiceClient,
            ILykkeAssetsResolver lykkeAssetsResolver
        )
        {
            _payInvoiceClient = payInvoiceClient;
            _lykkeAssetsResolver = lykkeAssetsResolver;
        }

        public async Task<PaymentsResponse> GetByPaymentsFilter(
            string merchantId,
            PaymentType type,
            IReadOnlyList<InvoiceStatus> statuses,
            PaymentsFilterPeriod period,
            string searchText,
            int? take
        )
        {
            var dates = period.GetDates();
            searchText = searchText?.Trim();

            // TODO: using of PaymentType

            var response = await _payInvoiceClient.GetByPaymentsFilter(
                 merchantId,
                 statuses.Select(_ => _.ToString()),
                 dates.DateFrom,
                 dates.DateTo,
                 searchText,
                 take);

            var result = new PaymentsResponse()
            {
                Payments = Mapper.Map<List<Payment>>(response.Invoices),
                HasMorePayments = response.HasMoreInvoices,
                HasAnyPayment = true
            };

            foreach (var payment in result.Payments)
            {
                payment.SettlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(payment.SettlementAssetId);
            }

            if (response.Invoices.Count == 0 &&
                statuses.Count == 0 &&
                period != PaymentsFilterPeriod.AllTime &&
                string.IsNullOrEmpty(searchText))
            {
                result.HasAnyPayment = await _payInvoiceClient.HasAnyInvoice(merchantId);
            }
            
            return result;
        }
    }
}
