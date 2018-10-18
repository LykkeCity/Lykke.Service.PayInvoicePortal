using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInvoicePortal.Models.Email
{
    public class EmailSendModel
    {
        [Required]
        public string InvoiceId { get; set; }

        [Required]
        public string CheckoutUrl { get; set; }
        
        [Required]
        [NotEmptyCollection]
        public List<string> Emails { get; set; }
    }
}
