using System.Collections.Generic;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain.Statistic;

namespace Lykke.Service.PayInvoicePortal.Core.Domain
{
    public class InvoiceSource
    {
        public int Total { get; set; }
        public IDictionary<InvoiceStatus, int> CountPerStatus { get; set; }
        public IDictionary<InvoiceStatus, double> MainStatistic { get; set; }
        public IEnumerable<SummaryStatisticModel> SummaryStatistic { get; set; }
        public IDictionary<string, double> Rates { get; set; }
        public bool HasErrorsInStatistic { get; set; }
        public double Balance { get; set; }
        public string BaseAsset { get; set; }
        public int BaseAssetAccuracy { get; set; }
        public List<Invoice> Items { get; set; }
        
        
    }
}
