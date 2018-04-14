using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.Order;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly HashSet<InvoiceStatus> _excludeStatuses = new HashSet<InvoiceStatus>
        {
            InvoiceStatus.InProgress,
            InvoiceStatus.RefundInProgress,
            InvoiceStatus.SettlementInProgress
        };

        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ILog _log;

        public InvoiceService(
            IPayInvoiceClient payInvoiceClient,
            IPayInternalClient payInternalClient,
            IAssetsServiceWithCache assetsService,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _payInternalClient = payInternalClient;
            _assetsService = assetsService;
            _log = log;
        }

        public async Task<Invoice> GetByIdAsync(string invoiceId)
        {
            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoice.SettlementAssetId);

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
                SettlementAsset = settlementAsset,
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
                    employee = await _payInvoiceClient.GetEmployeeAsync(merchantId, item.ModifiedById);
                }

                Asset historySettlementAsset = await _assetsService.TryGetAssetAsync(item.SettlementAssetId);
                Asset historyPeymentAsset = await _assetsService.TryGetAssetAsync(item.PaymentAssetId);

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
            IReadOnlyList<Invoice> result =
                await GetAsync(merchantId, period, searchValue, sortField, sortAscending);

            if (status.Count > 0)
            {
                result = result
                    .Where(o => status.Contains(o.Status))
                    .ToList();
            }

            return result;
        }

        public async Task<PaymentDetails> GetPaymentDetailsAsync(string invoiceId)
        {
            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            if (invoice.DueDate <= DateTime.UtcNow || invoice.Status == InvoiceStatus.Removed)
                return null;

            Task<MerchantModel> merchantTask = _payInternalClient.GetMerchantByIdAsync(invoice.MerchantId);
            Task<PaymentRequestModel> paymentRequestTask =
                _payInternalClient.GetPaymentRequestAsync(invoice.MerchantId, invoice.PaymentRequestId);
            Task<OrderModel> orderTask = _payInternalClient.ChechoutOrderAsync(new ChechoutRequestModel
            {
                MerchantId = invoice.MerchantId,
                PaymentRequestId = invoice.PaymentRequestId,
                Force = true
            });

            await Task.WhenAll(merchantTask, paymentRequestTask, orderTask);

            OrderModel order = orderTask.Result;
            PaymentRequestModel paymentRequest = paymentRequestTask.Result;
            MerchantModel merchant = merchantTask.Result;

            Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoice.SettlementAssetId);
            Asset paymentAsset = await _assetsService.TryGetAssetAsync(invoice.PaymentAssetId);

            int totalSeconds = 0;
            int remainingSeconds = 0;
            
            if (invoice.Status == InvoiceStatus.Unpaid)
            {
                totalSeconds = (int)(order.DueDate - order.CreatedDate).TotalSeconds;
                remainingSeconds = (int)(order.DueDate - DateTime.UtcNow).TotalSeconds;

                if (remainingSeconds > totalSeconds)
                    remainingSeconds = totalSeconds;
            }

            return new PaymentDetails
            {
                Id = invoice.Id,
                Number = invoice.Number,
                Status = invoice.Status,
                Merchant = merchant,
                PaymentAmount = order.PaymentAmount,
                SettlementAmount = invoice.Amount,
                PaymentAsset = paymentAsset,
                SettlementAsset = settlementAsset,
                ExchangeRate = order.ExchangeRate,
                DeltaSpread = merchant.DeltaSpread > 0,
                Pips = paymentRequest.MarkupPips + merchant.LpMarkupPips,
                Percents = merchant.DeltaSpread + paymentRequest.MarkupPercent + merchant.LpMarkupPercent,
                Fee = merchant.MarkupFixedFee + paymentRequest.MarkupFixedFee,
                DueDate = invoice.DueDate,
                Note = invoice.Note,
                WalletAddress = paymentRequest.WalletAddress,
                PaymentRequestId = invoice.PaymentRequestId,
                TotalSeconds = totalSeconds,
                RemainingSeconds = remainingSeconds,
                PaidAmount = paymentRequest.PaidAmount,
                PaidDate = paymentRequest.PaidDate
            };
        }

        public async Task<InvoiceStatus> GetStatusAsync(string invoiceId)
        {
            InvoiceModel invoice = await _payInvoiceClient.GetInvoiceAsync(invoiceId);

            return invoice.Status;
        }

        public async Task<Invoice> CreateAsync(CreateInvoiceModel model, bool draft)
        {
            InvoiceModel invoice;

            if (draft)
                invoice = await _payInvoiceClient.CreateDraftInvoiceAsync(model);
            else
                invoice = await _payInvoiceClient.CreateInvoiceAsync(model);

            Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoice.SettlementAssetId);

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
            await _payInvoiceClient.UpdateDraftInvoiceAsync(model);

            if (!draft)
            {
                await _payInvoiceClient.CreateInvoiceAsync(model.Id);
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
            IReadOnlyList<Invoice> result =
                await GetAsync(merchantId, period, searchValue, sortField, sortAscending);

            var source = new InvoiceSource
            {
                Total = result.Count,
                CountPerStatus = new Dictionary<InvoiceStatus, int>()
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

        private async Task<IReadOnlyList<Invoice>> GetAsync(
            string merchantId,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending)
        {
            IEnumerable<InvoiceModel> invoices = await _payInvoiceClient.GetMerchantInvoicesAsync(merchantId);

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
                Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoice.SettlementAssetId);

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
                    CreatedDate = invoice.CreatedDate,
                    Note = invoice.Note
                });
            }

            return items;
        }
    }
}
