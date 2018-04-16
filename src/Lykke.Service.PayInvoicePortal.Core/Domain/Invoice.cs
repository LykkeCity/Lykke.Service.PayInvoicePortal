using System;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Core.Domain
{
    public class Invoice
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public Asset SettlementAsset { get; set; }
        public string WalletAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Note { get; set; }
    }
}
