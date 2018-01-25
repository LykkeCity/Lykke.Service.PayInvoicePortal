using System.Threading.Tasks;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
