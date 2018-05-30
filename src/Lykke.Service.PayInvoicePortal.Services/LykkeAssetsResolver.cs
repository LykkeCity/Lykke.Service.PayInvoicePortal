using System.Threading.Tasks;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoicePortal.Core.Domain.Settings.AppSettings;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class LykkeAssetsResolver : ILykkeAssetsResolver
    {
        private const string _defaultPaymentAssetId = "BTC";
        private readonly AssetsMapSettings _assetsMap;
        private readonly IAssetsServiceWithCache _assetsService;

        public LykkeAssetsResolver(
            AssetsMapSettings assetsMap,
            IAssetsServiceWithCache assetsService)
        {
            _assetsMap = assetsMap;
            _assetsService = assetsService;
        }

        public async Task<Asset> TryGetAssetAsync(string assetId)
        {
            Asset asset = await _assetsService.TryGetAssetAsync(assetId);

            if (asset == null && _assetsMap.Values.TryGetValue(assetId, out string lykkeId))
            {
                asset = await _assetsService.TryGetAssetAsync(lykkeId);
            }

            return asset;
        }

        public string GetInvoiceCreationPair(string assetId)
        {
            if (_assetsMap.InvoiceCreationPairs.TryGetValue(assetId, out string paymentAssetId))
            {
                return paymentAssetId;
            }

            return _defaultPaymentAssetId;
        }
    }
}
