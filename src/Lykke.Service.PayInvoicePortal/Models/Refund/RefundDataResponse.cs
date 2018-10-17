using System.Collections.Generic;
using Lykke.Service.PayInvoicePortal.Models.Assets;

namespace Lykke.Service.PayInvoicePortal.Models.Refund
{
    public class RefundDataResponse
    {
        public AssetModel PaymentAsset { get; set; }

        public IEnumerable<string> SourceWalletAddresses { get; set; }

        public decimal Amount { get; set; }
    }
}
