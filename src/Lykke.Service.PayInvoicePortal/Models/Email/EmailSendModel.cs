using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Models.Email
{
    public class EmailSendModel
    {
        public string InvoiceId { get; set; }

        public string CheckoutUrl { get; set; }
        
        public List<string> Emails { get; set; }
    }
}
