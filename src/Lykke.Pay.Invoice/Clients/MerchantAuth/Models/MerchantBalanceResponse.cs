using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lykke.Pay.Invoice.Clients.MerchantAuth.Models
{
    public class MerchantBalanceResponse
    {
        [JsonProperty("asserts")]
        public List<MerchantAssetBalance> Assets { get; set; }
    }
}
