using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class PayInvoicesRequest
    {
        [Required]
        public IEnumerable<string> InvoicesIds { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string AssetForPay { get; set; }
    }
}
