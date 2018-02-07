using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Models.Home
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
        
        public IList<GridRowItem> Data { get; set; }
    }
}
