using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Balances.AutorestClient.Models;
using Lykke.Service.Balances.Client;
using Lykke.Service.Balances.Client.ResponseModels;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class BalanceService : IBalanceService
    {
        private const string AssetId = "CHF";

        private readonly IPayInternalClient _payInternalClient;
        private readonly IBalancesClient _balancesClient;
        private readonly ILog _log;

        public BalanceService(
            IPayInternalClient payInternalClient,
            IBalancesClient balancesClient,
            ILog log)
        {
            _payInternalClient = payInternalClient;
            _balancesClient = balancesClient;
            _log = log;
        }

        public async Task<Balance> GetAsync(string merchantId)
        {
            var balance = new Balance
            {
                Currency = AssetId
            };
            
            try
            {
                MerchantModel merchant = await _payInternalClient.GetMerchantByIdAsync(merchantId);

                if (string.IsNullOrEmpty(merchant?.LwId))
                    return balance;

                ClientBalanceModel clientBalance = await _balancesClient.GetClientBalanceByAssetId(
                    new ClientBalanceByAssetIdModel
                    {
                        AssetId = AssetId,
                        ClientId = merchant.LwId
                    });

                balance.Value = clientBalance?.Balance ?? 0;
            }
            catch (Exception exception)
            {
                _log.WriteError(nameof(GetAsync), new { merchantId, AssetId }, exception);
            }

            return balance;
        }
    }
}
