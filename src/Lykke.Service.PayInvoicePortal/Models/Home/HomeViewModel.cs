using Lykke.Service.PayInvoicePortal.Models.Balances;
using Lykke.Service.PayInvoicePortal.Models.Invoices;

namespace Lykke.Service.PayInvoicePortal.Models.Home
{
    public class HomeViewModel
    {
        public InvoicesGatheredInfoModel InvoicesGatheredInfo { get; set; }
        public BalanceModel Balance { get; set; }
    }
}
