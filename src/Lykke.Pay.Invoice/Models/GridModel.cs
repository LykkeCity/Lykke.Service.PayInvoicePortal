using System.Collections.Generic;
using Lykke.Pay.Service.Invoces.Client.Models.Invoice;

namespace Lykke.Pay.Invoice.Models
{
    public class GridModel
    {
        public GridModel()
        {
            Header = new GridHeader();
            Filter = new GridFilter();
        }
        public GridHeader Header { get; set; }
        public GridFilter Filter { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        
        public IList<InvoiceModel> Data { get; set; }
    }

    public class GridFilter
    {
        public int SortWay { get; set; }
        public string Status { get; set; }
        public string SearchValue { get; set; }
        public string SortField { get; set; }
        public int Period { get; set; }
    }

    public class GridHeader
    {
        public int AllCount { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public int RemovedCount { get; set; }
        public int InProgressCount { get; set; }
        public int OverpaidCount { get; set; }
        public int LatePaidCount { get; set; }
        public int UnderpaidCount { get; set; }
        public int DraftCount { get; set; }
    }
}
