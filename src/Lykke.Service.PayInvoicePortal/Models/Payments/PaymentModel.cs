using System;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Models.Assets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInvoicePortal.Models.Payments
{
    public class PaymentModel
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceStatus Status { get; set; }
        public AssetModel SettlementAsset { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentAssetId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
