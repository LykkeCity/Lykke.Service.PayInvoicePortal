namespace Lykke.Pay.Invoice.Models
{
    public class InvoiceResult
    {
        public string QRCode { get; set; }
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public double Amount { get; set; }
        public double OrigAmount { get; set; }
        public string Currency { get; set; }
    }
}