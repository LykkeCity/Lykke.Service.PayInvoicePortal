using System;
using System.Linq;
using System.Security.Claims;
using Lykke.Pay.Common.Entities.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Lykke.Pay.Invoice.Controllers
{
    public class BaseController : Controller
    {

        protected readonly string ConnectionStrings;
        protected readonly string LykkePayUrl;
        protected readonly string MerchantAuthService;
        protected readonly string SiteUrl;
        protected readonly string HomeUrl = "~/Home/Profile";
        protected readonly TimeSpan InvoiceLiveTime;
        protected readonly TimeSpan OrderLiveTime;

        private string _merchantId;
        private MerchantStaff _merchantStaff;
        protected string MerchantId {
            get
            {
                if (string.IsNullOrEmpty(_merchantId))
                {
                    var staff = User.Claims.First(u => u.Type == ClaimTypes.UserData).Value;
                    _merchantStaff = JsonConvert.DeserializeObject<MerchantStaff>(staff);
                    _merchantId = _merchantStaff.MerchantId;
                }

                return _merchantId;
            }
        }

        public BaseController(IConfiguration configuration)
        {
            ConnectionStrings = configuration.GetValue<string>("ConnectionStrings");
            LykkePayUrl = configuration.GetValue<string>("LykkePayUrl");
            MerchantAuthService = configuration.GetValue<string>("MerchantAuthService");
            InvoiceLiveTime = configuration.GetValue<TimeSpan>("InvoiceLiveTime");
            OrderLiveTime = configuration.GetValue<TimeSpan>("OrderLiveTime");
            SiteUrl = configuration.GetValue<string>("SiteUrl");
        }
    }
}
