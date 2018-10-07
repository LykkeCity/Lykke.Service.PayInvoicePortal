using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Core.Domain.Payments
{
    public class PaymentsResponse
    {
        public IReadOnlyList<Payment> Payments { get; set; }

        public bool HasMorePayments { get; set; }

        public bool HasAnyPayment { get; set; }
    }
}
