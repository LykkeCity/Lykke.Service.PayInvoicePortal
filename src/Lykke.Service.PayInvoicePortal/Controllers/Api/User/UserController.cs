using System.Net;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api.User
{
    [Authorize]
    [Route("api/user")]
    public class UserController : Controller
    {
        [HttpGet]
        [ProducesResponseType(typeof(UserInfoModel), (int)HttpStatusCode.OK)]
        public IActionResult GetUserInfo()
        {
            return Ok(new UserInfoModel
            {
                EmployeeId = User.GetEmployeeId(),
                EmployeeEmail = User.GetEmail(),
                FullName = User.GetName(),
                MerchantId = User.GetMerchantId()
            });
        }
    }
}
