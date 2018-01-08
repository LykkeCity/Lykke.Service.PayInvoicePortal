using Newtonsoft.Json;

namespace Lykke.Pay.Invoice.Clients.MerchantAuth.Models
{
    public class MerchantAssetBalance
    {
        [JsonProperty("assert")]
        public string Asset { get; set; }
        public double Value { get; set; }
    }
}
