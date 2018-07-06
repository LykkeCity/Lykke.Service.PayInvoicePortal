using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Core.Domain.Incoming
{
    public class IncomingInvoiceListItem
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MerchantName { get; set; }
        public decimal Amount { get; set; }
        public string SettlementAsset { get; set; }
        public int SettlementAssetAccuracy { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime DueDate { get; set; }
        public bool Dispute { get; set; }
    }
}
