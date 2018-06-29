using System.Collections.Generic;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain.Statistic;

namespace Lykke.Service.PayInvoicePortal.Core.Domain.Incoming
{
    public class IncomingInvoicesSource
    {
        public int Total { get; set; }
        public IDictionary<InvoiceStatus, int> CountPerStatus { get; set; }
        public IReadOnlyList<IncomingInvoiceListItem> Items { get; set; }
        public string BaseAsset { get; set; }
    }
}
