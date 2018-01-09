using Lykke.Pay.Service.Invoces.Client.Models.Invoice;

namespace Lykke.Pay.Invoice.Models
{
    public class InvoiceDetailModel
    {
        public string QRCode { get; set; }
        public string InvoiceUrl { get; set; }
        public InvoiceModel Data { get; set; }
    }
}
