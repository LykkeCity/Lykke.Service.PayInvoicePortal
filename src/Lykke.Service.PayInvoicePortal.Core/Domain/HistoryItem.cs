using System;
using System.Collections.Generic;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Core.Domain
{
    public class HistoryItem
    {
        public string AuthorFullName { get; set; }

        public InvoiceStatus Status { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal SettlementAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public Asset PaymentAsset { get; set; }

        public Asset SettlementAsset { get; set; }

        public decimal ExchangeRate { get; set; }

        public List<string> SourceWalletAddresses { get; set; }

        public string RefundWalletAddress { get; set; }

        public decimal RefundAmount { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? PaidDate { get; set; }

        public DateTime Date { get; set; }
    }
}
