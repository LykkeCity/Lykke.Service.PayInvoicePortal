using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Cache;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Markup;
using Lykke.Service.PayMerchant.Client.Models;
using Lykke.Service.PayInternal.Client.Models.Order;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Domain.Settings.ServiceSettings;
using Lykke.Service.PayInvoicePortal.Core.Domain.Statistic;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.RateCalculator.Client;
using Microsoft.Extensions.Caching.Memory;
using Lykke.Service.PayInternal.Client.Models.SupervisorMembership;
using Lykke.Service.PayInvoicePortal.Core.Domain.Incoming;
using Lykke.Common.Log;
using Lykke.Service.PayInvoicePortal.Services.Extensions;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Service.PayMerchant.Client;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly HashSet<InvoiceStatus> _excludeStatusesFromHistory = new HashSet<InvoiceStatus>
        {
            InvoiceStatus.InProgress
        };
        private readonly HashSet<InvoiceStatus> _getOnlyFirstStatusesFromHistory = new HashSet<InvoiceStatus>
        {
            InvoiceStatus.Unpaid,
            InvoiceStatus.Draft
        };
        private readonly IMerchantService _merchantService;
        private readonly IAssetService _assetService;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly IRateCalculatorClient _rateCalculatorClient;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriods;
        private readonly ILog _log;
        private readonly OnDemandDataCache<Tuple<double>> _ratesCache;
        private readonly OnDemandDataCache<string> _employeeFullNameCache;

        public InvoiceService(
            IMerchantService merchantService,
            IAssetService assetService,
            IPayInvoiceClient payInvoiceClient,
            IPayInternalClient payInternalClient,
            IRateCalculatorClient rateCalculatorClient,
            ILykkeAssetsResolver lykkeAssetsResolver,
            IMemoryCache memoryCache,
            CacheExpirationPeriodsSettings cacheExpirationPeriods,
            ILogFactory logFactory, 
            IPayMerchantClient payMerchantClient)
        {
            _merchantService = merchantService;
            _assetService = assetService;
            _payInvoiceClient = payInvoiceClient;
            _payInternalClient = payInternalClient;
            _rateCalculatorClient = rateCalculatorClient;
            _lykkeAssetsResolver = lykkeAssetsResolver;
            _cacheExpirationPeriods = cacheExpirationPeriods;
            _payMerchantClient = payMerchantClient;
            _ratesCache = new OnDemandDataCache<Tuple<double>>(memoryCache);
            _employeeFullNameCache = new OnDemandDataCache<string>(memoryCache);
            _log = logFactory.CreateLog(this);
        }

        public async Task<Invoice> GetByIdAsync(string invoiceId)
        {
            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            Asset settlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.SettlementAssetId);

            PaymentRequestModel paymentRequest = null;

            if (!string.IsNullOrEmpty(invoice.PaymentRequestId))
                paymentRequest =
                    await _payInternalClient.GetPaymentRequestAsync(invoice.MerchantId, invoice.PaymentRequestId);

            return new Invoice
            {
                Id = invoice.Id,
                Number = invoice.Number,
                ClientEmail = invoice.ClientEmail,
                ClientName = invoice.ClientName,
                Amount = invoice.Amount,
                DueDate = invoice.DueDate,
                Status = invoice.Status,
                SettlementAssetId = invoice.SettlementAssetId,
                SettlementAsset = settlementAsset,
                PaymentAssetId = invoice.PaymentAssetId,
                PaymentRequestId = invoice.PaymentRequestId,
                WalletAddress = paymentRequest?.WalletAddress,
                CreatedDate = invoice.CreatedDate,
                Note = invoice.Note
            };
        }

        public async Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(string merchantId, string invoiceId)
        {
            IReadOnlyList<HistoryItemModel> history = await _payInvoiceClient.GetInvoiceHistoryAsync(invoiceId);

            history = history
                .GroupBy(o => o.Status)
                .Where(o => !_excludeStatusesFromHistory.Contains(o.Key))
                .SelectMany(o => _getOnlyFirstStatusesFromHistory.Contains(o.Key) 
                    ? new List<HistoryItemModel> { o.OrderBy(s => s.Date).First() }
                    : o.ToList())
                .OrderBy(o => o.Date)
                .ToList();

            var items = new List<HistoryItem>();

            foreach (HistoryItemModel item in history)
            {
                var authorFullName = string.Empty;

                if (!string.IsNullOrEmpty(item.ModifiedById))
                {
                    authorFullName = await _employeeFullNameCache.GetOrAddAsync
                    (
                        $"EmployeeFullNameCache-{item.ModifiedById}",
                        async _ => {
                            try
                            {
                                var employee = await _payInvoiceClient.GetEmployeeAsync(item.ModifiedById);
                                return employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty;
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex);
                                return null;
                            }
                        }
                    );
                }

                Asset historySettlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(item.SettlementAssetId);
                Asset historyPeymentAsset = await _lykkeAssetsResolver.TryGetAssetAsync(item.PaymentAssetId);

                items.Add(new HistoryItem
                {
                    AuthorFullName = authorFullName,
                    Status = item.Status,
                    PaymentAmount = item.PaymentAmount,
                    SettlementAmount = item.SettlementAmount,
                    PaidAmount = item.PaidAmount,
                    PaymentAsset = historyPeymentAsset,
                    SettlementAsset = historySettlementAsset,
                    ExchangeRate = item.ExchangeRate,
                    SourceWalletAddresses = item.SourceWalletAddresses,
                    RefundWalletAddress = item.RefundWalletAddress,
                    RefundAmount = item.RefundAmount,
                    DueDate = item.DueDate,
                    PaidDate = item.PaidDate,
                    Date = item.Date
                });
            }

            return items;
        }

        public async Task<IReadOnlyList<FileInfoModel>> GetFilesAsync(string invoiceId)
        {
            IEnumerable<FileInfoModel> files = await _payInvoiceClient.GetFilesAsync(invoiceId);

            return files.ToList();
        }

        public async Task<IReadOnlyList<Invoice>> GetAsync(
            string merchantId,
            IReadOnlyList<InvoiceStatus> status,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending)
        {
            IEnumerable<InvoiceModel> allInvoices = await _payInvoiceClient.GetMerchantInvoicesAsync(merchantId);

            IReadOnlyList<Invoice> result =
                await FilterAsync(allInvoices, period, searchValue, sortField, sortAscending);

            if (status.Count > 0)
            {
                result = result
                    .Where(o => status.Contains(o.Status))
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Get payment details from Invoice, PaymentRequest and Order
        /// </summary>
        /// <param name="invoiceId">Invoice Id</param>
        /// <param name="force">Will force to create new order if the actual order is expired but can be considered
        //     as actual till extended due date</param>
        /// <returns></returns>
        public async Task<PaymentDetails> GetPaymentDetailsAsync(string invoiceId, bool force)
        {
            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            if (invoice.DueDate <= DateTime.UtcNow || invoice.Status == InvoiceStatus.Removed)
                return null;

            Task<MerchantModel> merchantTask = _payMerchantClient.Api.GetByIdAsync(invoice.MerchantId);

            Task<MarkupResponse> markupForMerchantTask = 
                _payInternalClient.ResolveMarkupByMerchantAsync(invoice.MerchantId, $"{invoice.PaymentAssetId}{invoice.SettlementAssetId}");

            // now it is important to wait order checkout before making GetPaymentRequest
            // as WalletAddress will be only after that
            Task<OrderModel> orderTask = _payInternalClient.ChechoutOrderAsync(new ChechoutRequestModel
            {
                MerchantId = invoice.MerchantId,
                PaymentRequestId = invoice.PaymentRequestId,
                Force = force
            });

            try
            {
                await Task.WhenAll(merchantTask, markupForMerchantTask, orderTask);
            }
            catch (Exception ex)
            {
                _log.ErrorWithDetails(ex, invoice.Sanitize());
                throw;
            }

            OrderModel order = orderTask.Result;
            MerchantModel merchant = merchantTask.Result;
            MarkupResponse markupForMerchant = markupForMerchantTask.Result;

            PaymentRequestModel paymentRequest =
                await _payInternalClient.GetPaymentRequestAsync(invoice.MerchantId, invoice.PaymentRequestId);

            Asset settlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.SettlementAssetId);
            Asset paymentAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.PaymentAssetId);

            int totalSeconds = 0;
            int remainingSeconds = 0;
            int extendedTotalSeconds = 0;
            int extendedRemainingSeconds = 0;
            
            if (invoice.Status == InvoiceStatus.Unpaid)
            {
                totalSeconds = (int)(order.DueDate - order.CreatedDate).TotalSeconds;
                remainingSeconds = (int)(order.DueDate - DateTime.UtcNow).TotalSeconds;

                if (remainingSeconds > totalSeconds)
                    remainingSeconds = totalSeconds;

                extendedTotalSeconds = (int)(order.ExtendedDueDate - order.DueDate).TotalSeconds;
                extendedRemainingSeconds = (int)(order.ExtendedDueDate - DateTime.UtcNow).TotalSeconds;

                if (extendedRemainingSeconds > extendedTotalSeconds)
                {
                    extendedRemainingSeconds = extendedTotalSeconds;
                }
            }

            return new PaymentDetails
            {
                Id = invoice.Id,
                Number = invoice.Number,
                Status = invoice.Status,
                Merchant = merchant,
                PaymentAmount = order.PaymentAmount,
                SettlementAmount = invoice.Amount,
                PaymentAssetId = invoice.PaymentAssetId,
                PaymentAsset = paymentAsset,
                SettlementAssetId = invoice.SettlementAssetId,
                SettlementAsset = settlementAsset,
                ExchangeRate = order.ExchangeRate,
                DeltaSpread = markupForMerchant.DeltaSpread > 0,
                Pips = markupForMerchant.Pips + paymentRequest.MarkupPips,
                Percents = markupForMerchant.DeltaSpread + markupForMerchant.Percent + paymentRequest.MarkupPercent,
                Fee = markupForMerchant.FixedFee + paymentRequest.MarkupFixedFee,
                DueDate = invoice.DueDate,
                Note = invoice.Note,
                WalletAddress = paymentRequest.WalletAddress,
                PaymentRequestId = invoice.PaymentRequestId,
                TotalSeconds = totalSeconds,
                RemainingSeconds = remainingSeconds,
                ExtendedTotalSeconds = extendedTotalSeconds,
                ExtendedRemainingSeconds = extendedRemainingSeconds,
                PaidAmount = paymentRequest.PaidAmount,
                PaidDate = paymentRequest.PaidDate
            };
        }

        public async Task<InvoiceStatusModel> GetStatusAsync(string invoiceId)
        {
            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            var model = new InvoiceStatusModel
            {
                Status = invoice.Status.ToString(),
                PaymentRequestId = invoice.PaymentRequestId
            };

            return model;
        }

        public async Task<InvoiceStatus> GetStatusOnlyAsync(string invoiceId)
        {
            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            return invoice.Status;
        }

        public async Task<InvoiceModel> ChangePaymentAssetAsync(string invoiceId, string paymentRequestId)
        {
            try
            {
                InvoiceModel invoice = await _payInvoiceClient.ChangePaymentAssetAsync(invoiceId, paymentRequestId);

                return invoice;
            }
            catch (ErrorResponseException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<Invoice> CreateAsync(CreateInvoiceModel model, bool draft)
        {
            model.PaymentAssetId = _lykkeAssetsResolver.GetInvoiceCreationPair(model.SettlementAssetId);

            InvoiceModel invoice;

            try
            {
                if (draft)
                    invoice = await _payInvoiceClient.CreateDraftInvoiceAsync(model);
                else
                    invoice = await _payInvoiceClient.CreateInvoiceAsync(model);
            }
            catch (ErrorResponseException ex)
            {
                _log.ErrorWithDetails(ex, new { model = model.Sanitize(), draft });

                throw new InvalidOperationException(ex.Message);
            }

            Asset settlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.SettlementAssetId);

            return new Invoice
            {
                Id = invoice.Id,
                Number = invoice.Number,
                ClientEmail = invoice.ClientEmail,
                ClientName = invoice.ClientName,
                Amount = invoice.Amount,
                DueDate = invoice.DueDate,
                Status = invoice.Status,
                SettlementAsset = settlementAsset,
                CreatedDate = invoice.CreatedDate,
                Note = invoice.Note
            };
        }

        public async Task UpdateAsync(UpdateInvoiceModel model, bool draft)
        {
            model.PaymentAssetId = _lykkeAssetsResolver.GetInvoiceCreationPair(model.SettlementAssetId);

            try
            {
                await _payInvoiceClient.UpdateDraftInvoiceAsync(model);

                if (!draft)
                {
                    await _payInvoiceClient.CreateInvoiceAsync(model.Id);
                }
            }
            catch (ErrorResponseException ex)
            {
                _log.ErrorWithDetails(ex, new { model = model.Sanitize(), draft });

                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task UploadFileAsync(string invoiceId, byte[] content, string fileName, string contentType)
        {
            await _payInvoiceClient.UploadFileAsync(invoiceId, content, fileName, contentType);
        }

        public async Task DeleteAsync(string invoiceId)
        {
            await _payInvoiceClient.DeleteInvoiceAsync(invoiceId);
        }

        public async Task<IncomingInvoicesSource> GetIncomingAsync(
            string merchantId,
            IReadOnlyList<InvoiceStatus> statuses,
            Period period,
            string searchValue,
            int skip,
            int take)
        {
            IReadOnlyList<string> groupMerchants = await _merchantService.GetGroupMerchantsAsync(merchantId);

            var invoices = await _payInvoiceClient.GetByFilter(groupMerchants, new string[] { merchantId }, statuses.Select(x => x.ToString()), null, null, null, null);

            IReadOnlyList<IncomingInvoiceListItem> result =
                await FilterInboxAsync(invoices, merchantId, period, searchValue);

            var source = new IncomingInvoicesSource
            {
                Total = result.Count,
                CountPerStatus = new Dictionary<InvoiceStatus, int>(),
                Items = result.Skip(skip).Take(take).ToList(),
                BaseAsset = await GetBaseAssetId(merchantId)
            };

            foreach (InvoiceStatus value in Enum.GetValues(typeof(InvoiceStatus)))
                source.CountPerStatus[value] = result.Count(o => o.Status == value);

            return source;
        }

        public async Task<InvoiceSource> GetSupervisingAsync(
            string merchantId,
            string employeeId,
            IReadOnlyList<InvoiceStatus> status,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending,
            int skip,
            int take)
        {
            MerchantsSupervisorMembershipResponse membership =
                await _payInternalClient.GetSupervisorMembershipWithMerchantsAsync(employeeId);

            var invoiceslist = new List<InvoiceModel>();

            if (membership?.Merchants.Any() ?? false)
            {
                foreach (var merchant in membership.Merchants)
                {
                    var invoices = await _payInvoiceClient.GetMerchantInvoicesAsync(merchant);

                    invoiceslist.AddRange(invoices);
                }
            }

            IReadOnlyList<Invoice> result =
                await FilterAsync(invoiceslist, period, searchValue, sortField, sortAscending);

            var source = new InvoiceSource
            {
                Total = result.Count,
                CountPerStatus = new Dictionary<InvoiceStatus, int>(),
                Balance = 0,
                MainStatistic = new Dictionary<InvoiceStatus, double>(),
                SummaryStatistic = Enumerable.Empty<SummaryStatisticModel>(),
                Rates = new Dictionary<string, double>(),
                HasErrorsInStatistic = false
            };

            if (status.Count > 0)
            {
                source.Items = result
                    .Where(o => status.Contains(o.Status))
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            }
            else
            {
                source.Items = result
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            }

            foreach (InvoiceStatus value in Enum.GetValues(typeof(InvoiceStatus)))
                source.CountPerStatus[value] = result.Count(o => o.Status == value);

            return source;
        }

        #region Private methods

        private async Task<string> GetBaseAssetId(string merchantId)
        {
            var baseAssetId = await _assetService.GetBaseAssetId(merchantId) ?? _assetService.GetDefaultBaseAssetId();

            return baseAssetId;
        }

        private async Task<IReadOnlyList<Invoice>> FilterAsync(
            IEnumerable<InvoiceModel> invoices,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending)
        {
            var query = invoices.AsQueryable();

            if (!string.IsNullOrEmpty(sortField))
            {
                query = sortAscending ? query.OrderBy(sortField) : query.OrderBy($"{sortField} descending");
            }
            else
            {
                query = query.OrderByDescending(o => o.CreatedDate);
            }

            query = FilterByPeriod(period, query);

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(o => (o.ClientEmail ?? string.Empty).ToLower().Contains(searchValue) ||
                                         (o.Number ?? string.Empty).ToLower().Contains(searchValue));
            }

            var items = new List<Invoice>();

            foreach (InvoiceModel invoice in query)
            {
                Asset settlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.SettlementAssetId);

                items.Add(new Invoice
                {
                    Id = invoice.Id,
                    MerchantId = invoice.MerchantId,
                    Number = invoice.Number,
                    ClientEmail = invoice.ClientEmail,
                    ClientName = invoice.ClientName,
                    Amount = invoice.Amount,
                    DueDate = invoice.DueDate,
                    Status = invoice.Status,
                    SettlementAsset = settlementAsset,
                    PaidAmount = invoice.PaidAmount,
                    PaymentAssetId = invoice.PaymentAssetId,
                    CreatedDate = invoice.CreatedDate,
                    Note = invoice.Note
                });
            }

            return items;
        }

        private async Task<IReadOnlyList<IncomingInvoiceListItem>> FilterInboxAsync(
            IEnumerable<InvoiceModel> invoices,
            string merchantId,
            Period period,
            string searchValue)
        {
            var query = invoices.AsQueryable();

            query = query.OrderByDescending(o => o.CreatedDate);

            query = FilterByPeriod(period, query);

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(o => (o.Number ?? string.Empty).ToLower().Contains(searchValue));
            }

            var items = new List<IncomingInvoiceListItem>();

            foreach (InvoiceModel invoice in query)
            {
                Asset settlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.SettlementAssetId);

                items.Add(new IncomingInvoiceListItem
                {
                    Id = invoice.Id,
                    Number = invoice.Number,
                    MerchantName = await _merchantService.GetMerchantNameAsync(invoice.MerchantId),
                    Amount = invoice.Amount,
                    DueDate = invoice.DueDate,
                    Status = invoice.Status,
                    SettlementAsset = settlementAsset?.DisplayId,
                    SettlementAssetAccuracy  = settlementAsset?.Accuracy ?? 0,
                    CreatedDate = invoice.CreatedDate,
                    Dispute = invoice.Dispute
                });
            }

            return items;
        }

        private static IQueryable<InvoiceModel> FilterByPeriod(Period period, IQueryable<InvoiceModel> query)
        {
            if (period != Period.AllTime)
            {
                DateTime dateFrom = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
                
                if (period == Period.LastMonth)
                    dateFrom = dateFrom.AddMonths(-1);

                if (period == Period.LastThreeMonths)
                    dateFrom = dateFrom.AddMonths(-3);

                query = query.Where(i => i.DueDate >= dateFrom);
            }

            return query;
        }

        #endregion
    }
}
