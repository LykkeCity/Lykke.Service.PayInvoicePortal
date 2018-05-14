using Lykke.Service.Balances.Client;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoicePortal.Settings.ServiceSettings;
using Lykke.Service.PayInvoicePortal.Settings.SlackNotifications;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoicePortal.Settings.Clients;

namespace Lykke.Service.PayInvoicePortal.Settings
{
    public class AppSettings
    {
        public PayInvoicePortalSettings PayInvoicePortal { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public PayInvoiceServiceClientSettings PayInvoiceServiceClient { get; set; }
        public RateCalculatorServiceClientSettings RateCalculatorServiceClient { get; set; }
        public PayAuthServiceClientSettings PayAuthServiceClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public PayInternalServiceClientSettings PayInternalServiceClient { get; set; }
        public EmailPartnerRouterServiceClientSettings EmailPartnerRouterServiceClient { get; set; }
        public BalancesServiceClientSettings BalancesServiceClient { get; set; }
    }
}
