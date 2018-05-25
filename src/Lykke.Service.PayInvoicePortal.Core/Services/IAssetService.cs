using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IAssetService
    {
        Task<IReadOnlyDictionary<string, string>> GetSettlementAssetsAsync(string merchantId);
        Task<IReadOnlyDictionary<string, string>> GetPaymentAssetsAsync(string merchantId, string settlementAssetId);
    }
}
