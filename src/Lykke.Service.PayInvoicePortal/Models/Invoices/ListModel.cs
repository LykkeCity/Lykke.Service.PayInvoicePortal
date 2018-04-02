using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class ListModel
    {
        public ListModel()
        {
            CountPerStatus = new Dictionary<string, int>();
            Items = new List<ListItemModel>();
        }

        public int Total { get; set; }
        public Dictionary<string, int> CountPerStatus { get; set; }
        public List<ListItemModel> Items { get; set; }
    }
}
