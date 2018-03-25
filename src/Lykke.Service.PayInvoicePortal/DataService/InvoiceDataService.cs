using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Controllers;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using InvoiceModel = Lykke.Service.PayInvoicePortal.Models.Invoices.InvoiceModel;

namespace Lykke.Service.PayInvoicePortal.DataService
{
    public class InvoiceDataService
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IInvoiceService _invoiceService;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ILog _log;

        public InvoiceDataService(
            IPayInvoiceClient payInvoiceClient,
            IPayInternalClient payInternalClient,
            IInvoiceService invoiceService,
            IAssetsServiceWithCache assetsService,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _payInternalClient = payInternalClient;
            _invoiceService = invoiceService;
            _assetsService = assetsService;
            _log = log;
        }

        public async Task<InvoiceModel> GetById(string invoiceId)
        {
            PayInvoice.Client.Models.Invoice.InvoiceModel invoice =
                await _payInvoiceClient.GetInvoiceAsync(invoiceId);
            IEnumerable<FileInfoModel> invoiceFiles = await _payInvoiceClient.GetFilesAsync(invoice.Id);
            Asset settlementAsset = await _assetsService.TryGetAssetAsync(invoice.SettlementAssetId);

            var model = new InvoiceModel
            {
                Id = invoice.Id,
                Number = invoice.Number,
                ClientEmail = invoice.ClientEmail,
                ClientName = invoice.ClientName,
                Amount = (double) invoice.Amount,
                DueDate = invoice.DueDate,
                Status = invoice.Status.ToString(),
                SettlementAsset = settlementAsset.DisplayId,
                SettlementAssetAccuracy = settlementAsset.Accuracy,
                CreatedDate = invoice.CreatedDate,
                Note = invoice.Note,
                Files = invoiceFiles
                    .Select(o => new FileModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Size = o.Size
                    })
                    .ToList()
            };

            return model;
        }

        public async Task<ListModel> GetListSourceAsync(
            string merchantId,
            string searchValue,
            Period period,
            List<PayInvoice.Client.Models.Invoice.InvoiceStatus> status,
            string sortField,
            bool sortAscending,
            int skip,
            int take)
        {
            InvoiceSource source = await _invoiceService.GetAsync(
                merchantId,
                status,
                period,
                searchValue,
                sortField,
                sortAscending,
                skip,
                take);

            var model = new ListModel
            {
                Total = source.Total,
                CountPerStatus = source.CountPerStatus.ToDictionary(o => o.Key.ToString(), o => o.Value)
            };

            foreach (PayInvoice.Client.Models.Invoice.InvoiceModel item in source.Items)
            {
                Asset settlementAsset = await _assetsService.TryGetAssetAsync(item.SettlementAssetId);

                model.Items.Add(new ListItemModel
                {
                    Id = item.Id,
                    Number = item.Number,
                    ClientEmail = item.ClientEmail,
                    ClientName = item.ClientName,
                    Amount = (double) item.Amount,
                    DueDate = item.DueDate,
                    Status = item.Status.ToString(),
                    SettlementAsset = settlementAsset.DisplayId,
                    SettlementAssetAccuracy = settlementAsset.Accuracy,
                    CreatedDate = item.CreatedDate
                });
            }

            return model;
        }

        public async Task<PaymentDetailsModel> GetPaymentDetailsAsync(string invoiceId)
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

            MerchantModel merchant = await _payInternalClient.GetMerchantByIdAsync(invoiceDetails.MerchantId);

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
                Merchant = merchant.Name,
                PaymentAmount = (double)invoiceDetails.PaymentAmount,
                SettlementAmount = (double)invoiceDetails.Amount,
                PaymentAsset = paymentAsset.DisplayId,
                SettlementAsset = settlementAsset.DisplayId,
                PaymentAssetAccuracy = paymentAsset.Accuracy,
                SettlementAssetAccuracy = settlementAsset.Accuracy,
                ExchangeRate = (double)invoiceDetails.ExchangeRate,
                SpreadPercent = invoiceDetails.DeltaSpread / 100,
                FeePercent = invoiceDetails.MarkupPercent / 100,
                DueDate = invoiceDetails.DueDate,
                Note = invoiceDetails.Note,
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
