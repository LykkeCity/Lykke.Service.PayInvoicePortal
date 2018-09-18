using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.PayInvoicePortal.Models.User
{
    public class SettingsResponse
    {
        public string MerchantDisplayName { get; set; }
        public string EmployeeFullname { get; set; }
        public string EmployeeEmail { get; set; }
        public IEnumerable<AssetItemViewModel> AvailableBaseAssets { get; set; } = new List<AssetItemViewModel>();
        public string BaseAssetId { get; set; }
        public string MerchantId { get; set; }
        public string MerchantApiKey { get; set; }
        public bool HasPublicKey { get; set; }
    }
}
