using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class AssetService : IAssetService
    {
        private const string DefaultBaseAssetId = "CHF";
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly ILog _log;

        public AssetService(
            IPayInternalClient payInternalClient,
            IPayInvoiceClient payInvoiceClient,
            ILykkeAssetsResolver lykkeAssetsResolver,
            ILogFactory logFactory)
        {
            _payInternalClient = payInternalClient;
            _payInvoiceClient = payInvoiceClient;
            _lykkeAssetsResolver = lykkeAssetsResolver;
            _log = logFactory.CreateLog(this);
        }

        public async Task<Asset> GetBaseAssetOrDefault(string merchantId)
        {
            var baseAssetId = await GetBaseAssetId(merchantId) ?? GetDefaultBaseAssetId();

            Asset baseAsset = await _lykkeAssetsResolver.TryGetAssetAsync(baseAssetId);

            return baseAsset;
        }

        public async Task<string> GetBaseAssetId(string merchantId)
        {
            string baseAssetId;

            try
            {
                baseAssetId = await _payInvoiceClient.GetBaseAssetAsync(merchantId);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                baseAssetId = null;
            }

            return baseAssetId;
        }

        public string GetDefaultBaseAssetId()
        {
            return DefaultBaseAssetId;
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
            catch (Exception ex)
            {
                _log.ErrorWithDetails(ex, new { merchantId });
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
            catch (Exception ex)
            {
                _log.ErrorWithDetails(ex, new { merchantId, settlementAssetId });
            }

            return result;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetPaymentAssetsAsync(string merchantId)
        {
            var result = new Dictionary<string, string>();

            try
            {
                var assetMerchantSettingsResponse = await _payInternalClient.GetAssetMerchantSettingsAsync(merchantId);

                if (assetMerchantSettingsResponse != null)
                {
                    var assets = assetMerchantSettingsResponse.PaymentAssets.Split(";");
                    foreach (string assetId in assets)
                    {
                        Asset asset = await _lykkeAssetsResolver.TryGetAssetAsync(assetId);
                        if (asset != null)
                        {
                            result.TryAdd(assetId, asset?.DisplayId ?? assetId);
                        }
                    }
                }

                if (!result.Any())
                {
                    var baseAssetId = await GetBaseAssetId(merchantId);

                    if (!string.IsNullOrEmpty(baseAssetId))
                    {
                        Asset asset = await _lykkeAssetsResolver.TryGetAssetAsync(baseAssetId);

                        if (asset != null)
                        {
                            result.TryAdd(baseAssetId, asset?.DisplayId ?? baseAssetId);
                        }
                    }
                }
            }
            catch (DefaultErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _log.ErrorWithDetails(ex, new { merchantId });
            }

            return result;

        }

            public async Task<IReadOnlyDictionary<string, BlockchainType>> GetAssetsNetworkAsync()
        {
            var result = new Dictionary<string, BlockchainType>();

            try
            {
                IEnumerable<AssetGeneralSettingsResponse> response =
                    await _payInternalClient.GetAssetGeneralSettingsAsync();

                result = response.ToDictionary(x => x.AssetDisplayId, x => x.Network);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return result;
        }
    }
}
