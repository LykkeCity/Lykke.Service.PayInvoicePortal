﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class AssetService : IAssetService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly ILog _log;

        public AssetService(
            IPayInternalClient payInternalClient,
            IAssetsServiceWithCache assetsServiceWithCache,
            ILog log)
        {
            _payInternalClient = payInternalClient;
            _assetsServiceWithCache = assetsServiceWithCache;
            _log = log;
        }

        public async Task<IReadOnlyList<Asset>> GetSettlementAssetsAsync(string merchantId)
        {
            var assets = new List<Asset>();

            try
            {
                AvailableAssetsResponse response =
                    await _payInternalClient.ResolveAvailableAssetsAsync(merchantId, AssetAvailabilityType.Settlement);

                foreach (string assetId in response.Assets)
                {
                    Asset asset = await _assetsServiceWithCache.TryGetAssetAsync(assetId);
                    assets.Add(asset);
                }
            }
            catch (Exception exception)
            {
                _log.WriteError(nameof(GetSettlementAssetsAsync), new {merchantId}, exception);
            }

            return assets;
        }
    }
}
