using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInvoicePortal.Settings.Clients
{
    public class AssetsServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
