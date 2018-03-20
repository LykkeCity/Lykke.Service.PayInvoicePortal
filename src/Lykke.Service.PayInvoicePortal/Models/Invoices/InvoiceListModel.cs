using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class InvoiceListModel
    {
        public int Total { get; set; }
        public Dictionary<string, int> CountPerStatus { get; set; }
        public List<InvoiceListItemModel> Items { get; set; }
    }
}
