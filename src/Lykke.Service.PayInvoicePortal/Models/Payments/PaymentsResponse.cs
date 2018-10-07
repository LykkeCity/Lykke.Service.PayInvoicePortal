using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Models.Payments
{
    public class PaymentsResponse
    {
        public IReadOnlyList<PaymentModel> Payments { get; set; }

        public bool HasMorePayments { get; set; }

        public bool HasAnyPayment { get; set; }
    }
}
