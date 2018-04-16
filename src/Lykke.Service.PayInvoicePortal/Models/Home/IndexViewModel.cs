using Lykke.Service.PayInvoicePortal.Models.Balances;
using Lykke.Service.PayInvoicePortal.Models.Invoices;

namespace Lykke.Service.PayInvoicePortal.Models.Home
{
    public class IndexViewModel
    {
        public ListModel List { get; set; }

        public BalanceModel Balance { get; set; }
    }
}
