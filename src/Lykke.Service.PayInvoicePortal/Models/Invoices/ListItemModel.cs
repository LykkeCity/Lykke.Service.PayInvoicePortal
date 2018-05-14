using System;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class ListItemModel
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public double Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string SettlementAsset { get; set; }
        public int SettlementAssetAccuracy { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentAssetId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
