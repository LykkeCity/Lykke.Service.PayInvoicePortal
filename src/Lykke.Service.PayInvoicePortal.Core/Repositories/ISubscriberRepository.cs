using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.Core.Domain;

namespace Lykke.Service.PayInvoicePortal.Core.Repositories
{
    public interface ISubscriberRepository
    {
        Task<ISubscriber> GetAsync(string email);

        Task<IEnumerable<ISubscriber>> GetAllAsync();

        Task<ISubscriber> CreateAsync(ISubscriber subscriber);
    }
}
