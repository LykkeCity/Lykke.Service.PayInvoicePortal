using System.Threading.Tasks;
using Lykke.Pay.Invoice.Clients.MerchantAuth.Models;

namespace Lykke.Pay.Invoice.Clients.MerchantAuth
{
    public interface IMerchantAuthServiceClient
    {
        Task<MerchantStaffSignInResponse> SignInAsync(MerchantStaffSignInRequest request);

        Task<MerchantBalanceResponse> GetBalanceAsync(string staffId);
    }
}
