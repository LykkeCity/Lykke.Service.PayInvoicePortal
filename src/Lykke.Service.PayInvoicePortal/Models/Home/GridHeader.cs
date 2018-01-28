namespace Lykke.Service.PayInvoicePortal.Models.Home
{
    public class GridHeader
    {
        public int AllCount { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public int RemovedCount { get; set; }
        public int InProgressCount { get; set; }
        public int OverpaidCount { get; set; }
        public int LatePaidCount { get; set; }
        public int UnderpaidCount { get; set; }
        public int DraftCount { get; set; }
    }
}
