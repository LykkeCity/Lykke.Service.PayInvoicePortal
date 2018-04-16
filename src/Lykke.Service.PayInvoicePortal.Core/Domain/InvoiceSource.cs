using System.Collections.Generic;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Core.Domain
{
    public class InvoiceSource
    {
        public int Total { get; set; }
        public Dictionary<InvoiceStatus, int> CountPerStatus { get; set; }
        public List<Invoice> Items { get; set; }
    }
}
