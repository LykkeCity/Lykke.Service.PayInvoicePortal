using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInvoicePortal.Core.Domain.Settings.ServiceSettings
{
    public class CacheExpirationPeriodsSettings
    {
        public TimeSpan BaseAsset { get; set; }
        public TimeSpan Rate { get; set; }
        public TimeSpan MerchantName { get; set; }
    }
}
