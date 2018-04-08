using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IAssetService
    {
        Task<IReadOnlyList<Asset>> GetSettlementAssetsAsync(string merchantId);
    }
}
