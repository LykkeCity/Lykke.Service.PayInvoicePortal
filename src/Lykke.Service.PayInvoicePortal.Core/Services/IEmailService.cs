using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string invoiceId, string checkoutUrl, IReadOnlyList<string> addresses);

        Task<bool> SendEmailConfirmationAsync(string fullName, string emailConfirmationLink, IReadOnlyList<string> addresses);
    }
}
