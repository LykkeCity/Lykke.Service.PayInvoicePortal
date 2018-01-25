namespace Lykke.Service.PayInvoicePortal.Models
{
    public class InvoiceRequest
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceId { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Label { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
        public string WalletAddress { get; set; }
        public string StartDate { get; set; }
    }
}
