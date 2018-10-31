using System.Threading.Tasks;
using Lykke.Service.PayInvoice.Contract.Invoice;
using Lykke.Service.PayInvoicePortal.RabbitSubscribers;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public interface IRealtimeNotificationsService
    {
        Task SendInvoiceUpdateAsync(InvoiceUpdateMessage message);
    }
}
