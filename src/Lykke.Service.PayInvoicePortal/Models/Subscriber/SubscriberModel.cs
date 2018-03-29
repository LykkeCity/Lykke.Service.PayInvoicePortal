using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInvoicePortal.Core.Domain;

namespace Lykke.Service.PayInvoicePortal.Models.Subscriber
{
    public class SubscriberModel : ISubscriber
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
