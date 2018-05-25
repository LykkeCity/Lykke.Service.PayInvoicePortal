using System.Collections.Generic;
using Lykke.Service.PayInvoicePortal.Core.Domain.Statistic;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices.Statistic
{
    public class StatisticModel
    {
        public StatisticModel()
        {
            MainStatistic = new Dictionary<string, double>();
            SummaryStatistic = new List<SummaryStatisticModel>();
            Rates = new Dictionary<string, double>();
        }

        public IDictionary<string, double> MainStatistic { get; set; }
        public IEnumerable<SummaryStatisticModel> SummaryStatistic { get; set; }
        public IDictionary<string, double> Rates { get; internal set; }
        public bool HasErrorsInStatistic { get; internal set; }
    }
}
