using System;
using System.Linq;
using System.Security.Claims;
using Lykke.Pay.Common.Entities.Entities;
using Lykke.Pay.Invoice.AppCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Lykke.Pay.Invoice.Controllers
{
    public class BaseController : Controller
    {

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
            set => _merchantId = value;
        }

        public BaseController(AppSettings settings)
        {
            LykkePayUrl = settings.PayInvoice.LykkePayUrl;
            MerchantAuthService = settings.PayInvoice.MerchantAuthService;
            InvoiceLiveTime = settings.PayInvoice.InvoiceLiveTime;
            OrderLiveTime = settings.PayInvoice.OrderLiveTime;
            SiteUrl = settings.PayInvoice.SiteUrl;
        }
    }
}
