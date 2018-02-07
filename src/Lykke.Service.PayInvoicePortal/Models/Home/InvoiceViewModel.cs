namespace Lykke.Service.PayInvoicePortal.Models.Home
{
    public class InvoiceViewModel
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string Amount { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
        public string SettlementAssetId { get; set; }
        public string CreatedDate { get; set; }
    }
}
