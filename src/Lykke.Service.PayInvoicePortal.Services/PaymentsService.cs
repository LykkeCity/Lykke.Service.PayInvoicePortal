using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Services.Extensions;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayInvoiceClient _payInvoiceClient;

        public PaymentsService(
            IPayInternalClient payInternalClient,
            IPayInvoiceClient payInvoiceClient,
            ILykkeAssetsResolver lykkeAssetsResolver
        )
        {
            _payInternalClient = payInternalClient;
            _payInvoiceClient = payInvoiceClient;
            _lykkeAssetsResolver = lykkeAssetsResolver;
        }

        public async Task<Payment> GetByInvoiceId(string invoiceId)
        {
            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            var payment = Mapper.Map<Payment>(invoice);

            payment.SettlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(payment.SettlementAssetId);

            return payment;
        }

        /// <summary>
        /// Gets payments - the combined result of invoices and payment requests
        /// </summary>
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

            Task<PayInvoice.Client.Models.Invoice.GetByPaymentsFilterResponse> invoicesTask = null;
            Task<PayInternal.Client.Models.PaymentRequest.GetByPaymentsFilterResponse> paymentRequestsTask = null;

            if (type == PaymentType.All || type == PaymentType.Invoice)
            {
                invoicesTask = _payInvoiceClient.GetByPaymentsFilterAsync(
                    merchantId,
                    statuses.Select(_ => _.ToString()),
                    dates.DateFrom,
                    dates.DateTo,
                    searchText,
                    take);
            }

            var isOnlyInvoices = statuses.Count == 1 && statuses.First() == InvoiceStatus.Draft;

            if ((type == PaymentType.All || type == PaymentType.Api) &&
                !isOnlyInvoices)
            {
                var paymentRequestStatuses = new List<PaymentRequestStatus>();
                var processingErrors = new List<PaymentRequestProcessingError>();

                foreach (var invoiceStatus in statuses)
                {
                    var converted = invoiceStatus.ToPaymentRequestStatus();

                    if (converted.PaymentRequestStatus == PaymentRequestStatus.None) continue;

                    if (!paymentRequestStatuses.Contains(converted.PaymentRequestStatus))
                    {
                        paymentRequestStatuses.Add(converted.PaymentRequestStatus);
                    }

                    if (converted.ProcessingErrors == null) continue;

                    foreach (var processingError in converted.ProcessingErrors)
                    {
                        if (processingError == PaymentRequestProcessingError.None) continue;

                        if (!processingErrors.Contains(processingError))
                        {
                            processingErrors.Add(processingError);
                        }
                    }
                }

                paymentRequestsTask = _payInternalClient.GetByPaymentsFilterAsync(
                    merchantId,
                    paymentRequestStatuses.Select(_ => _.ToString()),
                    processingErrors.Select(_ => _.ToString()),
                    dates.DateFrom,
                    dates.DateTo,
                    take);
            }

            var result = new PaymentsResponse
            {
                HasAnyPayment = true
            };

            switch (type)
            {
                case PaymentType.All:
                    if (invoicesTask != null || paymentRequestsTask != null)
                    {
                        await Task.WhenAll(invoicesTask ?? Task.CompletedTask, paymentRequestsTask ?? Task.CompletedTask);

                        var payments = Mapper.Map<List<Payment>>(invoicesTask?.Result.Invoices);
                        if (paymentRequestsTask != null)
                        {
                            payments.AddRange(Mapper.Map<List<Payment>>(paymentRequestsTask?.Result.PaymeentRequests));
                        }

                        result.Payments = payments.OrderByDescending(x => x.CreatedDate).ToList();
                        result.HasMorePayments = (invoicesTask?.Result.HasMoreInvoices ?? false) ||
                                                 (paymentRequestsTask?.Result.HasMorePaymentRequests ?? false);
                    }

                    break;
                case PaymentType.Invoice:
                    if (invoicesTask != null)
                    {
                        var invoicesTaskResponse = await invoicesTask;

                        result.Payments = Mapper.Map<List<Payment>>(invoicesTaskResponse.Invoices);
                        result.HasMorePayments = invoicesTaskResponse.HasMoreInvoices;
                    }

                    break;
                case PaymentType.Api:
                    if (paymentRequestsTask != null)
                    {
                        var paymentRequestsTaskResponse = await paymentRequestsTask;

                        result.Payments = Mapper.Map<List<Payment>>(paymentRequestsTaskResponse.PaymeentRequests);
                        result.HasMorePayments = paymentRequestsTaskResponse.HasMorePaymentRequests;
                    }

                    break;
            }

            foreach (var payment in result.Payments)
            {
                payment.SettlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(payment.SettlementAssetId);
            }

            if (result.Payments.Count == 0 &&
                type == PaymentType.All &&
                statuses.Count == 0 &&
                period != PaymentsFilterPeriod.AllTime &&
                string.IsNullOrEmpty(searchText))
            {
                var hasAnyInvoiceTask = _payInvoiceClient.HasAnyInvoiceAsync(merchantId);
                var hasAnyPaymentRequestTask = _payInternalClient.HasAnyPaymentRequestAsync(merchantId);

                await Task.WhenAll(hasAnyInvoiceTask, hasAnyPaymentRequestTask);

                result.HasAnyPayment = hasAnyInvoiceTask.Result || hasAnyPaymentRequestTask.Result;
            }

            return result;
        }
    }
}
