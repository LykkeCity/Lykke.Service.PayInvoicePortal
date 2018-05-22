using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface ILykkeAssetsResolver
    {
        Task<Asset> TryGetAssetAsync(string assetId);
    }
}
