using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Cache;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Markup;
using Lykke.Service.PayInternal.Client.Models.Merchant;
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
using Lykke.Service.RateCalculator.Client.AutorestClient.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Lykke.Service.PayInternal.Client.Models.Supervisor;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class InvoiceService : IInvoiceService
    {
        private const string DefaultBaseAssetId = "CHF";

        private readonly HashSet<InvoiceStatus> _excludeStatuses = new HashSet<InvoiceStatus>
        {
            InvoiceStatus.InProgress,
            InvoiceStatus.RefundInProgress
        };

        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IRateCalculatorClient _rateCalculatorClient;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriods;
        private readonly ILog _log;
        private readonly OnDemandDataCache<Tuple<double>> _ratesCache;
        private readonly OnDemandDataCache<Tuple<string>> _baseAssetCache;

        public InvoiceService(
            IPayInvoiceClient payInvoiceClient,
            IPayInternalClient payInternalClient,
            IRateCalculatorClient rateCalculatorClient,
            ILykkeAssetsResolver lykkeAssetsResolver,
            IMemoryCache memoryCache,
            CacheExpirationPeriodsSettings cacheExpirationPeriods,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _payInternalClient = payInternalClient;
            _rateCalculatorClient = rateCalculatorClient;
            _lykkeAssetsResolver = lykkeAssetsResolver;
            _cacheExpirationPeriods = cacheExpirationPeriods;
            _ratesCache = new OnDemandDataCache<Tuple<double>>(memoryCache);
            _baseAssetCache = new OnDemandDataCache<Tuple<string>>(memoryCache);
            _log = log.CreateComponentScope(nameof(InvoiceService));
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
                .Where(o => !_excludeStatuses.Contains(o.Key))
                .Select(o => o.OrderByDescending(s => s.Date).First())
                .OrderBy(o => o.Date)
                .ToList();

            var items = new List<HistoryItem>();

            foreach (HistoryItemModel item in history)
            {
                EmployeeModel employee = null;

                if (!string.IsNullOrEmpty(item.ModifiedById))
                {
                    // TODO: use cache
                    employee = await _payInvoiceClient.GetEmployeeAsync(item.ModifiedById);
                }

                Asset historySettlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(item.SettlementAssetId);
                Asset historyPeymentAsset = await _lykkeAssetsResolver.TryGetAssetAsync(item.PaymentAssetId);

                items.Add(new HistoryItem
                {
                    Author = employee,
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
                await FilterAsync(allInvoices, merchantId, period, searchValue, sortField, sortAscending);

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

            Task<MerchantModel> merchantTask = _payInternalClient.GetMerchantByIdAsync(invoice.MerchantId);

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
                _log.WriteError(nameof(GetPaymentDetailsAsync), invoice, ex);
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
                Percents = markupForMerchant.DeltaSpread + markupForMerchant.Percent + (decimal)paymentRequest.MarkupPercent,
                Fee = markupForMerchant.FixedFee + (decimal)paymentRequest.MarkupFixedFee,
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

            if (draft)
                invoice = await _payInvoiceClient.CreateDraftInvoiceAsync(model);
            else
                invoice = await _payInvoiceClient.CreateInvoiceAsync(model);

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

        public async Task<InvoiceSource> GetAsync(
            string merchantId,
            IReadOnlyList<InvoiceStatus> status,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending,
            int skip,
            int take)
        {
            IEnumerable<InvoiceModel> allInvoices = await _payInvoiceClient.GetMerchantInvoicesAsync(merchantId);

            var baseAssetIdTuple = await _baseAssetCache.GetOrAddAsync
                (
                    $"BaseAssetId-{merchantId}",
                    async x => {
                        var baseAssetIdResponse = await GetBaseAssetId(merchantId);
                        return new Tuple<string>(baseAssetIdResponse);
                    },
                    _cacheExpirationPeriods.BaseAsset
                );
            var baseAssetId = baseAssetIdTuple.Item1;
            Asset baseAsset = await _lykkeAssetsResolver.TryGetAssetAsync(baseAssetId);

            #region Statistic
            var rateDictionary = new Dictionary<string, double>();

            var allAssetIds = new List<string>();
            allAssetIds.AddRange(allInvoices.Select(x => x.PaymentAssetId));
            allAssetIds.AddRange(allInvoices.Select(x => x.SettlementAssetId));
            allAssetIds = allAssetIds.Distinct().ToList();

            foreach (var assetId in allAssetIds)
            {
                if (assetId == baseAssetId)
                {
                    rateDictionary.Add(assetId, 1);
                    continue;
                }

                var rateTuple = await _ratesCache.GetOrAddAsync
                (
                    $"Rate-{assetId}-{baseAssetId}",
                    async x => {
                        var asset = await _lykkeAssetsResolver.TryGetAssetAsync(assetId);
                        var rateResponse = asset == null || baseAsset == null ? 0 : await _rateCalculatorClient.GetAmountInBaseAsync(
                            assetFrom: asset.Id, amount: 1d, assetTo: baseAsset.Id);
                        return new Tuple<double>(rateResponse);
                    },
                    _cacheExpirationPeriods.Rate
                );

                var rate = rateTuple.Item1;

                if (rate == 0)
                {
                    _log.WriteWarning(nameof(GetAsync), null, $"No rates from {assetId} to {baseAssetId}");
                }

                rateDictionary.Add(assetId, rate);
            }

            bool IsOtherPaidStatuses(InvoiceStatus invoiceStatus)
            {
                return invoiceStatus == InvoiceStatus.Overpaid
                    || invoiceStatus == InvoiceStatus.Underpaid
                    || invoiceStatus == InvoiceStatus.LatePaid;
            }

            var mainStatistic = new Dictionary<InvoiceStatus, double>();
            var summaryStatistic = new Dictionary<InvoiceStatus, SummaryStatisticModel>();
            var grouppedByStatus = allInvoices.GroupBy(x => x.Status);

            double balance = 0;
            
            foreach (var group in grouppedByStatus)
            {
                var summaryStatisticGrouppedByAsset = new Dictionary<string, SummaryStatisticItemModel>();
                
                foreach (var invoice in group)
                {
                    var amount = IsOtherPaidStatuses(invoice.Status)
                        ? (double)invoice.PaidAmount * rateDictionary[invoice.PaymentAssetId]
                        : (double)invoice.Amount * rateDictionary[invoice.SettlementAssetId];

                    if (mainStatistic.ContainsKey(invoice.Status))
                    {
                        mainStatistic[invoice.Status] += amount;
                    }
                    else
                    {
                        mainStatistic.Add(invoice.Status, amount);
                    }

                    //TODO make calculation for OtherPaidStatuses in balance task
                    if (group.Key == InvoiceStatus.Paid
                        || IsOtherPaidStatuses(group.Key))
                    {
                        balance += amount;
                    }

                    // summary
                    if (summaryStatisticGrouppedByAsset.ContainsKey(invoice.SettlementAssetId))
                    {
                        var model = summaryStatisticGrouppedByAsset[invoice.SettlementAssetId];
                        model.Total += invoice.Amount;
                        model.Count++;
                    }
                    else
                    {
                        Asset settlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.SettlementAssetId);

                        summaryStatisticGrouppedByAsset.Add(invoice.SettlementAssetId,
                            new SummaryStatisticItemModel
                            {
                                Asset = settlementAsset?.DisplayId,
                                AssetAccuracy = settlementAsset?.Accuracy ?? 2,
                                Total = invoice.Amount,
                                Count = 1
                            });
                    }
                }

                summaryStatistic.Add(group.Key,
                    new SummaryStatisticModel
                    {
                        Status = group.Key.ToString(),
                        Items = summaryStatisticGrouppedByAsset.OrderBy(x => x.Key).Select(x => x.Value)
                    });
            }

            // order by status
            summaryStatistic = summaryStatistic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            #endregion

            IReadOnlyList<Invoice> result =
                await FilterAsync(allInvoices, merchantId, period, searchValue, sortField, sortAscending);

            var source = new InvoiceSource
            {
                Total = result.Count,
                CountPerStatus = new Dictionary<InvoiceStatus, int>(),
                Balance = balance,
                BaseAsset = baseAssetId,
                BaseAssetAccuracy = baseAsset?.Accuracy ?? 2,
                MainStatistic = mainStatistic,
                SummaryStatistic = summaryStatistic.Values,
                Rates = rateDictionary,
                HasErrorsInStatistic = rateDictionary.ContainsValue(0)
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
            var supervising = new SupervisingMerchantsResponse();
            try
            {
                supervising = await _payInternalClient.GetSupervisingMerchantsAsync(merchantId, employeeId);
            }
            catch (Exception ex)
            {

            }
            var invoiceslist = new List<InvoiceModel>();
            if (supervising.Merchants != null)
            {
                foreach (var merchant in supervising.Merchants)
                {
                    var invoices = await _payInvoiceClient.GetMerchantInvoicesAsync(merchant);
                    invoiceslist.AddRange(invoices);
                }
            }
            IEnumerable<InvoiceModel> allInvoices = invoiceslist;
            var baseAssetIdTuple = await _baseAssetCache.GetOrAddAsync
                (
                    $"BaseAssetId-{merchantId}",
                    async x => {
                        var baseAssetIdResponse = await GetBaseAssetId(merchantId);
                        return new Tuple<string>(baseAssetIdResponse);
                    },
                    _cacheExpirationPeriods.BaseAsset
                );
            var baseAssetId = baseAssetIdTuple.Item1;
            Asset baseAsset = await _lykkeAssetsResolver.TryGetAssetAsync(baseAssetId);

            #region Statistic
            var rateDictionary = new Dictionary<string, double>();

            var allAssetIds = new List<string>();
            allAssetIds.AddRange(allInvoices.Select(x => x.PaymentAssetId));
            allAssetIds.AddRange(allInvoices.Select(x => x.SettlementAssetId));
            allAssetIds = allAssetIds.Distinct().ToList();

            foreach (var assetId in allAssetIds)
            {
                if (assetId == baseAssetId)
                {
                    rateDictionary.Add(assetId, 1);
                    continue;
                }

                var rateTuple = await _ratesCache.GetOrAddAsync
                (
                    $"Rate-{assetId}-{baseAssetId}",
                    async x => {
                        var asset = await _lykkeAssetsResolver.TryGetAssetAsync(assetId);
                        var rateResponse = await _rateCalculatorClient.GetAmountInBaseAsync(
                            assetFrom: asset.Id, amount: 1d, assetTo: baseAsset.Id);
                        return new Tuple<double>(rateResponse);
                    },
                    _cacheExpirationPeriods.Rate
                );

                var rate = rateTuple.Item1;

                if (rate == 0)
                {
                    _log.WriteWarning(nameof(GetAsync), null, $"No rates from {assetId} to {baseAssetId}");
                }

                rateDictionary.Add(assetId, rate);
            }

            bool IsPaidStatus(InvoiceStatus invoiceStatus)
            {
                return invoiceStatus == InvoiceStatus.Paid
                    || invoiceStatus == InvoiceStatus.Overpaid
                    || invoiceStatus == InvoiceStatus.Underpaid
                    || invoiceStatus == InvoiceStatus.LatePaid;
            }

            var mainStatistic = new Dictionary<InvoiceStatus, double>();
            var summaryStatistic = new Dictionary<InvoiceStatus, SummaryStatisticModel>();
            var grouppedByStatus = allInvoices.GroupBy(x => x.Status);

            double balance = 0;

            foreach (var group in grouppedByStatus)
            {
                var summaryStatisticGrouppedByAsset = new Dictionary<string, SummaryStatisticItemModel>();

                foreach (var invoice in group)
                {
                    var amount = IsPaidStatus(invoice.Status)
                        ? (double)invoice.PaidAmount * rateDictionary[invoice.PaymentAssetId]
                        : (double)invoice.Amount * rateDictionary[invoice.SettlementAssetId];

                    if (mainStatistic.ContainsKey(invoice.Status))
                    {
                        mainStatistic[invoice.Status] += amount;
                    }
                    else
                    {
                        mainStatistic.Add(invoice.Status, amount);
                    }

                    if (IsPaidStatus(group.Key))
                    {
                        balance += amount;
                    }

                    // summary
                    if (summaryStatisticGrouppedByAsset.ContainsKey(invoice.SettlementAssetId))
                    {
                        var model = summaryStatisticGrouppedByAsset[invoice.SettlementAssetId];
                        model.Total += invoice.Amount;
                        model.Count++;
                    }
                    else
                    {
                        Asset settlementAsset = await _lykkeAssetsResolver.TryGetAssetAsync(invoice.SettlementAssetId);

                        summaryStatisticGrouppedByAsset.Add(invoice.SettlementAssetId,
                            new SummaryStatisticItemModel
                            {
                                Asset = settlementAsset.DisplayId,
                                AssetAccuracy = settlementAsset?.Accuracy ?? 2,
                                Total = invoice.Amount,
                                Count = 1
                            });
                    }
                }

                summaryStatistic.Add(group.Key,
                    new SummaryStatisticModel
                    {
                        Status = group.Key.ToString(),
                        Items = summaryStatisticGrouppedByAsset.OrderBy(x => x.Key).Select(x => x.Value)
                    });
            }

            // order by status
            summaryStatistic = summaryStatistic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            #endregion

            IReadOnlyList<Invoice> result =
                await FilterAsync(allInvoices, merchantId, period, searchValue, sortField, sortAscending);

            var source = new InvoiceSource
            {
                Total = result.Count,
                CountPerStatus = new Dictionary<InvoiceStatus, int>(),
                Balance = balance,
                BaseAsset = baseAssetId,
                BaseAssetAccuracy = baseAsset.Accuracy,
                MainStatistic = mainStatistic,
                SummaryStatistic = summaryStatistic.Values,
                Rates = rateDictionary,
                HasErrorsInStatistic = rateDictionary.ContainsValue(0)
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

        private async Task<string> GetBaseAssetId(string merchantId)
        {
            var baseAssetId = string.Empty;

            try
            {
                var merchantSetting = await _payInvoiceClient.GetMerchantSettingAsync(merchantId);
                baseAssetId = merchantSetting.BaseAsset;
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                baseAssetId = DefaultBaseAssetId;
            }

            return baseAssetId;
        }

        private async Task<IReadOnlyList<Invoice>> FilterAsync(
            IEnumerable<InvoiceModel> invoices,
            string merchantId,
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

            if (period != Period.AllTime)
            {
                DateTime dateFrom = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);

                if (period == Period.LastMonth)
                    dateFrom = dateFrom.AddMonths(-1);

                if (period == Period.LastThreeMonths)
                    dateFrom = dateFrom.AddMonths(-3);

                query = query.Where(i => i.DueDate >= dateFrom);
            }

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
    }
}
