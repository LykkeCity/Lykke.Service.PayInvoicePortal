using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInvoicePortal.Settings.ServiceSettings
{
    public class RabbitSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string InvoiceUpdateExchangeName { get; set; }
    }
}
