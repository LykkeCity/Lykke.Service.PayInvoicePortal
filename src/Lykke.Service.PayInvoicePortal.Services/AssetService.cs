using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class AssetService : IAssetService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public AssetService(
            IPayInternalClient payInternalClient,
            ILykkeAssetsResolver lykkeAssetsResolver,
            ILog log)
        {
            _payInternalClient = payInternalClient;
            _lykkeAssetsResolver = lykkeAssetsResolver;
            _log = log.CreateComponentScope(nameof(AssetService));
        }

        public async Task<IReadOnlyDictionary<string, string>> GetSettlementAssetsAsync(string merchantId)
        {
            var result = new Dictionary<string, string>();

            try
            {
                AvailableAssetsResponse response =
                    await _payInternalClient.GetAvailableSettlementAssetsAsync(merchantId);

                foreach (string assetId in response.Assets)
                {
                    Asset asset = await _lykkeAssetsResolver.TryGetAssetAsync(assetId);

                    result.TryAdd(assetId, asset?.DisplayId ?? assetId);
                }
            }
            catch (Exception exception)
            {
                _log.WriteError(nameof(GetSettlementAssetsAsync), new {merchantId}, exception);
            }

            return result;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetPaymentAssetsAsync(string merchantId, string settlementAssetId)
        {
            var result = new Dictionary<string, string>();

            try
            {
                AvailableAssetsResponse response =
                    await _payInternalClient.GetAvailablePaymentAssetsAsync(merchantId, settlementAssetId);

                foreach (string assetId in response.Assets)
                {
                    Asset asset = await _lykkeAssetsResolver.TryGetAssetAsync(assetId);

                    result.TryAdd(assetId, asset?.DisplayId ?? assetId);
                }
            }
            catch (Exception exception)
            {
                _log.WriteError(nameof(GetPaymentAssetsAsync), new { merchantId }, exception);
            }

            return result;
        }
    }
}
