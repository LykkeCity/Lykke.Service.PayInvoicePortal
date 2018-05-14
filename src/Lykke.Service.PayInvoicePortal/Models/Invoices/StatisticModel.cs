using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class StatisticModel
    {
        public StatisticModel()
        {
            MainStatistic = new Dictionary<string, double>();
        }

        public Dictionary<string, double> MainStatistic { get; set; }
        public Dictionary<string, double> Rates { get; internal set; }
        public bool HasErrorsInStatistic { get; internal set; }
    }
}
