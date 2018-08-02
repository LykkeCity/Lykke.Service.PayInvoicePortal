namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class InvoiceCsvRowModel
    {
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string DueDate { get; set; }
        public string CreatedDate { get; set; }
    }
}
