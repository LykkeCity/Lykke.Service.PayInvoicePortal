using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInvoicePortal.Models.Subscriber
{
    public class SubscriberModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
