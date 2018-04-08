using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class HistoryItemModel
    {
        public string Author { get; set; }

        public string Status { get; set; }

        public double PaymentAmount { get; set; }

        public double SettlementAmount { get; set; }

        public double PaidAmount { get; set; }

        public string PaymentAsset { get; set; }

        public string SettlementAsset { get; set; }

        public int PaymentAssetAccuracy { get; set; }

        public int SettlementAssetAccuracy { get; set; }

        public double ExchangeRate { get; set; }

        public List<string> SourceWalletAddresses { get; set; }

        public string RefundWalletAddress { get; set; }

        public decimal RefundAmount { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? PaidDate { get; set; }

        public DateTime Date { get; set; }
    }
}
