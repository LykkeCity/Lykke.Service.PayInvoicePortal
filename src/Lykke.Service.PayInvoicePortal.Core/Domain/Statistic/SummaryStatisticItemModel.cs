namespace Lykke.Service.PayInvoicePortal.Core.Domain.Statistic
{
    public class SummaryStatisticItemModel
    {
        public string Asset { get; set; }
        public int AssetAccuracy { get; set; }
        public decimal Total { get; set; }
        public int Count { get; set; }
    }
}
