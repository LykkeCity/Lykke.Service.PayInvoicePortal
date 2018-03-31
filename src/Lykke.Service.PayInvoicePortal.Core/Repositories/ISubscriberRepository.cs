using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.Core.Domain;

namespace Lykke.Service.PayInvoicePortal.Core.Repositories
{
    public interface ISubscriberRepository
    {
        Task<Subscriber> GetAsync(string email);

        Task InsertAsync(Subscriber subscriber);
    }
}
