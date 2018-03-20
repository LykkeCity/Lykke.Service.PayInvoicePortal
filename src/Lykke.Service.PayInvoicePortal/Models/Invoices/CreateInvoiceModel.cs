using System;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class CreateInvoiceModel
    {
        public bool IsDraft { get; set; }
        public string Number { get; set; }
        public string Client { get; set; }
        public string Email { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public DateTime DueDate { get; set; }
        public string Note { get; set; }
    }
}
