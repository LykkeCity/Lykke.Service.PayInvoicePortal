using Lykke.Pay.Service.Invoces.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Pay.Invoice.Models
{
    public class InvoiceDetailModel
    {
        public string QRCode { get; set; }
        public string InvoiceUrl { get; set; }
        public IInvoiceEntity Data { get; set; }
    }
}
