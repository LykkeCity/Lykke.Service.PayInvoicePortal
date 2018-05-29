using System.Collections.Generic;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain.Statistic;

namespace Lykke.Service.PayInvoicePortal.Core.Domain
{
    public class InvoiceSource
    {
        public int Total { get; set; }
        public Dictionary<InvoiceStatus, int> CountPerStatus { get; set; }
        public Dictionary<InvoiceStatus, double> MainStatistic { get; set; }
        public IEnumerable<SummaryStatisticModel> SummaryStatistic { get; set; }
        public Dictionary<string, double> Rates { get; set; }
        public bool HasErrorsInStatistic { get; set; }
        public double Balance { get; set; }
        public string BaseAsset { get; set; }
        public int BaseAssetAccuracy { get; set; }
        public List<Invoice> Items { get; set; }
        
        
    }
}
