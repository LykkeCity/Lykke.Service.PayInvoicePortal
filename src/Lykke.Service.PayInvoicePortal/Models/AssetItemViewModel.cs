using Lykke.Service.PayInternal.Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInvoicePortal.Models
{
    public class AssetItemViewModel
    {
        public AssetItemViewModel()
        {
        }

        public AssetItemViewModel(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public string Id { get; set; }

        public string Title { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Network { get; set; }
    }
}
