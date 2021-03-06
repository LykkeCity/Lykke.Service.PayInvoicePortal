﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client.Models;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IAssetService
    {
        Task<Asset> GetBaseAssetOrDefault(string merchantId);
        Task<string> GetBaseAssetId(string merchantId);
        string GetDefaultBaseAssetId();
        Task<IReadOnlyDictionary<string, string>> GetSettlementAssetsAsync(string merchantId);
        Task<IReadOnlyDictionary<string, string>> GetPaymentAssetsAsync(string merchantId, string settlementAssetId);
        Task<IReadOnlyDictionary<string, string>> GetPaymentAssetsAsync(string merchantId);
        Task<BlockchainType> GetAssetNetworkAsync(string assetId);
    }
}
