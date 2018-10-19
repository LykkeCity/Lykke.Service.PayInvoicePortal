using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInvoicePortal.Models.Refund
{
    public class RefundRequest
    {
        [Required]
        public string PaymentRequestId { get; set; }

        [Required]
        public string DestinationAddress { get; set; }
    }
}
