using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.Core.Domain;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IBalanceService
    {
        Task<Balance> GetAsync(string merchantId);
    }
}
