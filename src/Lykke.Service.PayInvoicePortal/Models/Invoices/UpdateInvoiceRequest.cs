using System;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class UpdateInvoiceRequest : CreateInvoiceRequest
    {
        public string Id { get; set; }
    }
}
