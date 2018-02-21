using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    public class BaseController : Controller
    {
        private string _merchantId;
        private string _employeeId;

        protected string SiteUrl
            => Startup.SiteUrl;

        protected string BlockchainExplorerUrl
            => Startup.BlockchainExplorerUrl;

        protected string EmployeeId
            => _employeeId ?? (_employeeId = User.Claims.First(u => u.Type == ClaimTypes.Sid).Value);

        protected string MerchantId
            => _merchantId ?? (_merchantId = User.Claims.First(u => u.Type == ClaimTypes.UserData).Value);
        
    }
}
