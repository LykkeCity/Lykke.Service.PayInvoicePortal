using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayAuth.Client.Models.GenerateRsaKeys;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoice.Client.Models.MerchantSetting;
using Lykke.Service.PayInvoice.Core.Domain;
using Lykke.Service.PayInvoicePortal.Constants;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.User;
using Lykke.Service.PayMerchant.Client;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ErrorResponse = Lykke.Common.Api.Contract.Responses.ErrorResponse;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api.User
{
    [Authorize]
    [Route("api/changePassword")]
    public class ChangePasswordController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public ChangePasswordController(
            IAuthService authService,
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient,
            ILogFactory logFactory)
        {
            _authService = authService;
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _log = logFactory.CreateLog(this);
        }

        [HttpPost]
        [ValidateModel]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var email = User.GetEmail();

            var result = await _authService.ValidateAsync(email, model.CurrentPassword);

            var employee = result.Employee;

            if (employee == null)
            {
                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.ChangePassword.InvalidCurrentPassword));
            }

            try
            {
                await _payAuthClient.UpdateAsync(new UpdateCredentialsModel
                {
                    Email = employee.Email,
                    EmployeeId = employee.Id,
                    MerchantId = employee.MerchantId,
                    Password = model.NewPassword
                });
            }
            catch (Exception e)
            {
                _log.Error(e);

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _log.Info($"ChangePassword success for employeeId: {employee.Id} by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            return NoContent();
        }
    }
}
