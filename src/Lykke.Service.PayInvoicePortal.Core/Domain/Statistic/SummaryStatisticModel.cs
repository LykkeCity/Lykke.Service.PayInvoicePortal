using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Core.Domain.Statistic
{
    public class SummaryStatisticModel
    {
        public SummaryStatisticModel()
        {
            Items = new List<SummaryStatisticItemModel>();
        }

        public string Status { get; set; }
        public IEnumerable<SummaryStatisticItemModel> Items { get; set; }
    }
}
