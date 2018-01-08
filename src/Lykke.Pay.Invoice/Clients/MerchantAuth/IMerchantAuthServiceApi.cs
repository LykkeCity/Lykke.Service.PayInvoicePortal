using System.Threading.Tasks;
using Lykke.Pay.Invoice.Clients.MerchantAuth.Models;
using Refit;

namespace Lykke.Pay.Invoice.Clients.MerchantAuth
{
    public interface IMerchantAuthServiceApi
    {
        [Post("/api/merchant/staffSignIn")]
        Task<MerchantStaffSignInResponse> SignInAsync([Body] MerchantStaffSignInRequest body);

        [Get("/api/merchant/balance/{staffId}")]
        Task<MerchantBalanceResponse> GetBalanceAsync(string staffId);
    }
}
