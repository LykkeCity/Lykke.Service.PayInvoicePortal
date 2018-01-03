using System;
using JetBrains.Annotations;
using Lykke.AzureQueueIntegration;

namespace Lykke.Pay.Invoice.AppCode
{
    public class AppSettings
    {
        public PayInvoiceSettings PayInvoice { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }

    public class PayInvoiceSettings
    {
        public int UserLoginTime { get; set; }
        public TimeSpan InvoiceLiveTime { get; set; }
        public string LykkePayUrl { get; set; }
        public string SiteUrl { get; set; }
        public string MerchantAuthService { get; set; }
        public string InvoicesService { get; set; }
        public TimeSpan OrderLiveTime { get; set; }
        public LogsSettings Logs { get; set; }
    }


    public class LogsSettings
    {
        public string LogsConnString { get; set; }
    }

    [UsedImplicitly]
    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }

        public int ThrottlingLimitSeconds { get; set; }
    }
}
