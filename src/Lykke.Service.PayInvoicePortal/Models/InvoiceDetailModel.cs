using System.Collections.Generic;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Models
{
    public class InvoiceDetailModel
    {
        public string QRCode { get; set; }
        public string InvoiceUrl { get; set; }
        public string BlockchainExplorerUrl { get; set; }
        public InvoiceModel Data { get; set; }
        public List<FileModel> Files { get; set; }
    }
}
