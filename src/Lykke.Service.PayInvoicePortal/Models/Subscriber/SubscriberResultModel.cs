namespace Lykke.Service.PayInvoicePortal.Models.Subscriber
{
    public class SubscriberResultModel
    {
        public SubscriberResultModel()
        {
        }

        public SubscriberResultModel(string message)
        {
            Message = message;
            Error = true;
        }

        public string Message { get; set; }

        public bool Error { get; set; }
    }
}
