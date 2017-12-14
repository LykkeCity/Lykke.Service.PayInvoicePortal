using Lykke.AzureRepositories;
using Lykke.AzureRepositories.Azure.Tables;
using Lykke.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Lykke.Pay.Invoice.Controllers
{
    public class BaseController : Controller
    {

        protected readonly string ConnectionStrings;
        protected readonly string LykkePayUrl;
        protected readonly string MerchantId;
        protected readonly string MerchantApiKey;
        protected readonly string MerchantPrivateKey;
        protected readonly string MerchantAuthService;
        protected readonly string HomeUrl = "~/Home/Welcome";


        public BaseController(IConfiguration configuration)
        {
            ConnectionStrings = configuration.GetValue<string>("ConnectionStrings");
            LykkePayUrl = configuration.GetValue<string>("LykkePayUrl");
            MerchantId = configuration.GetValue<string>("MerchantId");
            MerchantApiKey = configuration.GetValue<string>("MerchantApiKey");
            MerchantPrivateKey = configuration.GetValue<string>("MerchantPrivateKey");
            MerchantAuthService = configuration.GetValue<string>("MerchantAuthService");

        }
    }
}
