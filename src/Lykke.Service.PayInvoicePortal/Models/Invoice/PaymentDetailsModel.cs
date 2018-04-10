using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Models.Invoice
{
    public class PaymentDetailsModel
    {
        public string Id { get; set; }

        public string Number { get; set; }

        public string Status { get; set; }

        public string Merchant { get; set; }

        public double PaymentAmount { get; set; }

        public double SettlementAmount { get; set; }

        public string PaymentAsset { get; set; }

        public string SettlementAsset { get; set; }

        public int PaymentAssetAccuracy { get; set; }

        public int SettlementAssetAccuracy { get; set; }

        public double ExchangeRate { get; set; }

        public bool DeltaSpread { get; set; }

        public double Percents { get; set; }

        public int Pips { get; set; }

        public double Fee { get; set; }

        public DateTime DueDate { get; set; }

        public string Note { get; set; }

        public string WalletAddress { get; set; }

        public string PaymentRequestId { get; set; }

        public int TotalSeconds { get; set; }

        public int RemainingSeconds { get; set; }

        public DateTime? PaidDate { get; set; }

        public double PaidAmount { get; set; }

        public IReadOnlyList<FileModel> Files { get; set; }
    }
}
