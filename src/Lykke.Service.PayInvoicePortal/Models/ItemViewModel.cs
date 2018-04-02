namespace Lykke.Service.PayInvoicePortal.Models
{
    public class ItemViewModel
    {
        public ItemViewModel()
        {
        }

        public ItemViewModel(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public string Id { get; set; }

        public string Title { get; set; }
    }
}
