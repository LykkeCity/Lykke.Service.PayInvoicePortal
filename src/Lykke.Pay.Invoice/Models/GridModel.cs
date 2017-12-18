using Lykke.Pay.Service.Invoces.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Pay.Invoice.Models
{
    public class GridModel
    {
        public int AllCount { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public int DraftCount { get; set; }
        public string SearchValue { get; set; }
        public string SortField { get; set; }
        public int Page { get; set; }
        public int SortWay { get; set; }
        public IList<IInvoiceEntity> Data { get; set; }
    }
}
