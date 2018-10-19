using System;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Core.Domain.Payments
{
    public class Payment
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public string SettlementAssetId { get; set; }
        public Asset SettlementAsset { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentAssetId { get; set; }
    }
}
