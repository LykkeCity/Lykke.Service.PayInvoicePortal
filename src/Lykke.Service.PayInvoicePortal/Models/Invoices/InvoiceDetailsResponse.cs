namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class InvoiceDetailsResponse
    {
        public InvoiceModel Invoice { get; set; }

        public string BlockchainExplorerUrl { get; set; }

        public string EthereumBlockchainExplorerUrl { get; set; }
    }
}
