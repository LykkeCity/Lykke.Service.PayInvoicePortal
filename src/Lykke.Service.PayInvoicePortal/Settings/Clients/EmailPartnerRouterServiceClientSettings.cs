using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInvoicePortal.Settings.Clients
{
    public class EmailPartnerRouterServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
