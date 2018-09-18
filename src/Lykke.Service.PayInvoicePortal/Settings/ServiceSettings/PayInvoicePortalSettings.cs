using System;
using Lykke.Service.PayInvoicePortal.Core.Domain.Settings.ServiceSettings;
using Lykke.Service.PayInvoicePortal.Settings.ServiceSettings.Db;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInvoicePortal.Settings.ServiceSettings
{
    public class PayInvoicePortalSettings
    {
        public string BlockchainExplorerUrl { get; set; }
        public string EthereumBlockchainExplorerUrl { get; set; }
        [Optional]
        public string PortalTestnetUrl { get; set; }
        public string ApiaryDocsDomain { get; set; }
        public TimeSpan UserLoginTime { get; set; }
       
        public DbSettings Db { get; set; }

        public TimeSpan AssetsCacheExpirationPeriod { get; set; }
        public CacheExpirationPeriodsSettings CacheExpirationPeriods { get; set; }
        [Optional]
        public bool EnableSignup { get; set; }
    }
}
