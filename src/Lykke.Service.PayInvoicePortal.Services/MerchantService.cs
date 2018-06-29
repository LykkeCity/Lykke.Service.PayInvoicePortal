using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.Cache;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.MerchantGroups;
using Lykke.Service.PayInvoicePortal.Core.Domain.Settings.ServiceSettings;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly IPayInternalClient _payInternalClient;
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriods;
        private readonly OnDemandDataCache<string> _merchantNamesCache;

        public MerchantService(
            IPayInternalClient payInternalClient,
            IMemoryCache memoryCache,
            CacheExpirationPeriodsSettings cacheExpirationPeriods)
        {
            _payInternalClient = payInternalClient;
            _cacheExpirationPeriods = cacheExpirationPeriods;
            _merchantNamesCache = new OnDemandDataCache<string>(memoryCache);
        }

        public async Task<string> GetMerchantNameAsync(string merchantId)
        {
            var merchantName = await _merchantNamesCache.GetOrAddAsync
                (
                    $"MerchantName-{merchantId}",
                    async x => {
                        var merchant = await _payInternalClient.GetMerchantByIdAsync(merchantId);
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
