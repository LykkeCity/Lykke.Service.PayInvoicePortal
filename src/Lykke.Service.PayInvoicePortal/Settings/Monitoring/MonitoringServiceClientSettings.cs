using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInvoicePortal.Settings.Monitoring
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
