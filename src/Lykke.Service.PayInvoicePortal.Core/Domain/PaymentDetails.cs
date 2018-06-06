using System;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Core.Domain
{
    public class PaymentDetails
    {
        public string Id { get; set; }

        public string Number { get; set; }

        public InvoiceStatus Status { get; set; }

        public MerchantModel Merchant { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal SettlementAmount { get; set; }

        /// <summary>
        /// In UI we must operate with PaymentAssetId stored in invoice
        /// </summary>
        public string PaymentAssetId { get; set; }

        public Asset PaymentAsset { get; set; }

        /// <summary>
        /// In UI we must operate with SettlementAssetId stored in invoice
        /// </summary>
        public string SettlementAssetId { get; set; }

        public Asset SettlementAsset { get; set; }

        public decimal ExchangeRate { get; set; }

        public bool DeltaSpread { get; set; }

        public decimal Percents { get; set; }

        public int Pips { get; set; }

        public decimal Fee { get; set; }

        public DateTime DueDate { get; set; }

        public string Note { get; set; }

        public string WalletAddress { get; set; }

        public string PaymentRequestId { get; set; }

        public int TotalSeconds { get; set; }

        public int RemainingSeconds { get; set; }

        public int ExtendedTotalSeconds { get; set; }

        public int ExtendedRemainingSeconds { get; set; }

        public DateTime? PaidDate { get; set; }

        public double PaidAmount { get; set; }
    }
}
