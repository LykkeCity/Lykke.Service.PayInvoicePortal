using System;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Pay.Invoice.Clients.MerchantAuth.Models;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.Pay.Invoice.Clients.MerchantAuth
{
    public class MerchantAuthServiceClient : IDisposable, IMerchantAuthServiceClient
    {
        public string HostUrl { get; }

        private readonly HttpClient _httpClient;
        private readonly IMerchantAuthServiceApi _api;

        public MerchantAuthServiceClient(string hostUrl)
        {
            HostUrl = hostUrl ?? throw new ArgumentNullException(nameof(hostUrl));

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(hostUrl),
                DefaultRequestHeaders =
                {
                    {
                        "User-Agent",
                        $"{PlatformServices.Default.Application.ApplicationName}/{PlatformServices.Default.Application.ApplicationVersion}"
                    }
                }
            };

            _api = RestService.For<IMerchantAuthServiceApi>(_httpClient);
        }

        public async Task<MerchantStaffSignInResponse> SignInAsync(MerchantStaffSignInRequest request)
        {
            return await RunAsync(() => _api.SignInAsync(request));
        }

        public async Task<MerchantBalanceResponse> GetBalanceAsync(string staffId)
        {
            return await RunAsync(() => _api.GetBalanceAsync(staffId));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private async Task<T> RunAsync<T>(Func<Task<T>> method)
        {
            try
            {
                return await method();
            }
            catch (ApiException exception)
            {
                throw new ErrorResponseException("An error occurred  during calling api", exception);
            }
        }
    }
}
