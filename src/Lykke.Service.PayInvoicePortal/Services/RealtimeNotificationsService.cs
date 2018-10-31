using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInvoice.Contract.Invoice;
using Lykke.Service.PayInvoicePortal.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class RealtimeNotificationsService : IRealtimeNotificationsService
    {
        private readonly IHubContext<InvoiceUpdateHub> _invoiceUpdateHub;
        private readonly ILog _log;

        public RealtimeNotificationsService(
            IHubContext<InvoiceUpdateHub> invoiceUpdateHub,
            ILogFactory logFactory)
        {
            _invoiceUpdateHub = invoiceUpdateHub;
            _log = logFactory.CreateLog(this);
        }

        public async Task SendInvoiceUpdateAsync(InvoiceUpdateMessage message)
        {
            _log.Info("InvoiceUpdateMessage: " + message.ToJson());

            await _invoiceUpdateHub.Clients.Group(message.MerchantId).SendAsync("invoiceUpdated", message);
        }
    }
}
