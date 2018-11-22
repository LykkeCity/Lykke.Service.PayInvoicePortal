using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Core.Domain.Payments
{
    public class PaymentsResponse
    {
        public PaymentsResponse()
        {
            Payments = new List<Payment>();   
        }

        public IReadOnlyList<Payment> Payments { get; set; }

        public bool HasMorePayments { get; set; }

        public bool HasAnyPayment { get; set; }
    }
}
