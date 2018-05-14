namespace Lykke.Service.PayInvoicePortal.Models.Home.Template
{
    public class HomeStatisticItemModel
    {
        public HomeStatisticItemModel(string status)
        {
            Status = status;
        }

        public string Status { get; set; }
    }
}
