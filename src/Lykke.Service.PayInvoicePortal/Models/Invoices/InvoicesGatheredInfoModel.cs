namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class InvoicesGatheredInfoModel
    {
        public ListModel List { get; set; }
        public double Balance { get; set; }
        public string BaseAsset { get; set; }
        public int BaseAssetAccuracy { get; set; }
        public StatisticModel Statistic { get; set; }
    }
}
