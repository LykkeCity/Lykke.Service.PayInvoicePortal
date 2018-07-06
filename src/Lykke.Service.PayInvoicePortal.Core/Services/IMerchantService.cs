using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IMerchantService
    {
        Task<string> GetMerchantNameAsync(string merchantId);
        Task<IReadOnlyList<string>> GetGroupMerchantsAsync(string merchantId);
    }
}
