using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class InvoiceStatusModel
    {
        public string Status { get; set; }
        public string PaymentRequestId { get; set; }
    }
}
