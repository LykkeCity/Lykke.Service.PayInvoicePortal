namespace Lykke.Pay.Invoice.Models
{
    public class NewInvoiceModel
    {
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string StartDate { get; set; }
        public string Status { get; set; }
    }
}
