using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.Cache;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.MerchantGroups;
using Lykke.Service.PayInvoicePortal.Core.Domain.Settings.ServiceSettings;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayMerchant.Client;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriods;
        private readonly OnDemandDataCache<string> _merchantNamesCache;

        public MerchantService(
            IPayInternalClient payInternalClient,
            IMemoryCache memoryCache,
            CacheExpirationPeriodsSettings cacheExpirationPeriods, 
            IPayMerchantClient payMerchantClient)
        {
            _payInternalClient = payInternalClient;
            _cacheExpirationPeriods = cacheExpirationPeriods;
            _payMerchantClient = payMerchantClient;
            _merchantNamesCache = new OnDemandDataCache<string>(memoryCache);
        }

        public async Task<string> GetMerchantNameAsync(string merchantId)
        {
            var merchantName = await _merchantNamesCache.GetOrAddAsync
                (
                    $"MerchantName-{merchantId}",
                    async x => {
                        var merchant = await _payMerchantClient.Api.GetByIdAsync(merchantId);
                        return merchant.DisplayName;
                    },
                    _cacheExpirationPeriods.MerchantName
                );

            return merchantName;
        }
        public async Task<IReadOnlyList<string>> GetGroupMerchantsAsync(string merchantId)
        {
            MerchantsByUsageResponse response = await _payInternalClient.GetMerchantsByUsageAsync(
                new GetMerchantsByUsageRequest
                {
                    MerchantId = merchantId,
                    MerchantGroupUse = MerchantGroupUse.Billing
                });

            return response.Merchants.ToList();
        }
    }
}
