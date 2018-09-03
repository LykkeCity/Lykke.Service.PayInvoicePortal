using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoicePortal.Models.User;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ErrorResponseException = Lykke.Service.PayAuth.Client.ErrorResponseException;

namespace Lykke.Service.PayInvoicePortal.Controllers.User
{
    public class ResetPasswordController : Controller
    {
        private const string ViewPath = "../User/ResetPassword";
        private const int TokenHexLength = 40;

        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;
        private readonly Regex _tokenRegex = new Regex(@"[\dA-F]+");

        public ResetPasswordController(
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient,
            ILogFactory logFactory)
        {
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [Route("resetPassword")]
        public async Task<IActionResult> ResetPassword(string token)
        {
            _log.Info($"ResetPassword opened by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            var validateTokenResult = await ValidateToken(token);

            if (!validateTokenResult.IsValidToken)
            {
                _log.Warning($"Invalid token provided: {token}");

                ModelState.AddModelError(string.Empty, "Invalid token");
            }

            ViewBag.Token = token;

            return View(ViewPath);
        }

        [HttpPost]
        [Route("api/resetPassword")]
        [ValidateModel]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var validateTokenResult = await ValidateToken(model.Token);

            if (!validateTokenResult.IsValidToken)
            {
                _log.Warning($"Invalid token provided: {model.Token}");

                return BadRequest(ErrorResponse.Create("Invalid token"));
            }

            try
            {
                var employee = await _payInvoiceClient.GetEmployeeAsync(validateTokenResult.ResetPasswordToken.EmployeeId);

                await _payAuthClient.UpdateAsync(new UpdateCredentialsModel
                {
                    Email = employee.Email,
                    EmployeeId = employee.Id,
                    MerchantId = employee.MerchantId,
                    Password = model.Password
                });

                await _payAuthClient.RedeemResetPasswordTokenAsync(model.Token);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Error occured for token: {model.Token}");

                return BadRequest(ErrorResponse.Create("Error occured"));
            }

            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            _log.Info($"ResetPassword success for token {model.Token} by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            return Ok();
        }

        private async Task<(bool IsValidToken, ResetPasswordTokenModel ResetPasswordToken)> ValidateToken(string token)
        {
            if (!IsValidTokenFormat(token)) return (false, null);

            try
            {
                var resetPasswordToken = await _payAuthClient.GetResetPasswordTokenByPublicIdAsync(token);

                var isValidToken = !resetPasswordToken.Redeemed && resetPasswordToken.ExpiresOn >= DateTime.UtcNow;

                return (isValidToken, resetPasswordToken);
            }
            catch (ErrorResponseException e)
            {
                _log.Warning($"Error on validating token: {token}", e);

                return (false, null);
            }
        }

        private bool IsValidTokenFormat(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length != TokenHexLength)
                return false;

            return _tokenRegex.IsMatch(token);
        }
    }
}
