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
        /// <summary>
        /// Possible values are (case insensitive): DEV, TEST, SANDBOX
        /// </summary>
        [Optional]
        public string DeploymentEnvironment { get; set; }
        public string ApiaryDocsDomain { get; set; }
        public TimeSpan UserLoginTime { get; set; }
       
        public DbSettings Db { get; set; }
        public RabbitSettings Rabbit { get; set; }
        public TimeSpan AssetsCacheExpirationPeriod { get; set; }
        public CacheExpirationPeriodsSettings CacheExpirationPeriods { get; set; }
        [Optional]
        public bool EnableSignup { get; set; }
    }

    public enum DeploymentEnvironment
    {
        Prod = 0,
        Dev,
        Test,
        Sandbox
    }
}
