using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Models.Invoice
{
    public class InvoiceViewModel
    {
        public string QRCode { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public string Amount { get; set; }
        public string OrigAmount { get; set; }
        public string Currency { get; set; }
        public int RefreshSeconds { get; set; }
        public bool AutoUpdate { get; set; }
        public string WalletAddress { get; set; }
        public InvoiceStatus Status { get; set; }
    }
}
