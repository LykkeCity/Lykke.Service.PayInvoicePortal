using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IPaymentsService
    {
        Task<PaymentsResponse> GetByPaymentsFilter(
            string merchantId,
            PaymentType type,
            IReadOnlyList<InvoiceStatus> statuses,
            PaymentsFilterPeriod period,
            string searchText,
            int? take
        );
    }
}
