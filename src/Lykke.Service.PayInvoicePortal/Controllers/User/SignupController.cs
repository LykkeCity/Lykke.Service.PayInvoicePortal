﻿using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoicePortal.Constants;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models.User;
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.PayMerchant.Client.Models;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ErrorResponseException = Lykke.Service.PayInvoice.Client.ErrorResponseException;

namespace Lykke.Service.PayInvoicePortal.Controllers.User
{
    public class SignupController : Controller
    {
        private const int TokenLength = 48;

        private readonly IPayMerchantClient _payMerchantClient;
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ISignupService _signupService;
        private readonly IEmailService _emailService;
        private readonly ILog _log;

        public SignupController(
            IPayMerchantClient payMerchantClient,
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient,
            ISignupService signupService,
            IEmailService emailService,
            ILogFactory logFactory)
        {
            _payMerchantClient = payMerchantClient;
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _signupService = signupService;
            _emailService = emailService;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [Route("signup")]
        public IActionResult Signup()
        {
            _log.Info($"Signup opened by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            if (!_signupService.EnableSignup)
                RedirectToAction("Index", "Error");

            return View("../User/Signup");
        }

        [HttpPost]
        [Route("api/signup")]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SignupNewMerchant([FromBody] SignupModel model)
        {
            if (!_signupService.EnableSignup)
                return Forbid();

            _log.Info($"Signup started for merchant {model.MerchantName} by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            var merchantId = _signupService.GetIdFromName(model.MerchantName);

            #region Validate

            try
            {
                var merchant = await _payMerchantClient.Api.GetByIdAsync(merchantId);

                if (merchant != null)
                    return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.Signup.MerchantExist));
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            { }

            try
            {
                var employee = await _payInvoiceClient.GetEmployeeByEmailAsync(model.EmployeeEmail);

                if (employee != null)
                    return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.Signup.EmployeeEmailExist));
            }
            catch (ErrorResponseException e) when (e.StatusCode == HttpStatusCode.NotFound)
            { }

            #endregion

            try
            {
                var apiKey = StringUtils.GenerateId();

                // create merchant
                var merchant = await _payMerchantClient.Api.CreateAsync(new CreateMerchantRequest
                {
                    Name = merchantId,
                    DisplayName = model.MerchantName,
                    ApiKey = apiKey,
                    Email = model.EmployeeEmail
                });

                await _payAuthClient.RegisterAsync(new RegisterRequest
                {
                    ApiKey = apiKey,
                    ClientId = merchant.Id
                });

                // create employee
                var employee = await _payInvoiceClient.AddEmployeeAsync(new CreateEmployeeModel
                {
                    Email = model.EmployeeEmail,
                    FirstName = model.EmployeeFirstName,
                    LastName = model.EmployeeLastName,
                    MerchantId = merchant.Id
                });

                await _payAuthClient.RegisterAsync(new RegisterModel
                {
                    EmployeeId = employee.Id,
                    MerchantId = merchant.Id,
                    Email = model.EmployeeEmail,
                    Password = model.EmployeePassword,
                    ForceEmailConfirmation = true
                });

                var token = employee.Id.ToBase64();

                var sent = await _emailService.SendEmailConfirmationAsync(
                    $"{employee.FirstName} {employee.LastName}",
                    $"{model.HostUrl.TrimEnd('/')}/signup/confirmEmail/{token}",
                    new[]
                    {
                        employee.Email
                    }
                );

                if (!sent)
                {
                    return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.Signup.EmailNotSent));
                }
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.BadRequest &&
                                               e.ErrorResponse.ErrorMessage ==
                                               "Merchant with the same email already exists")
            {
                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.Signup.MerchantEmailExist));
            }
            catch (Exception e)
            {
                _log.Error(e, $"Error occured for merchant {model.MerchantName}, id: {merchantId}");

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }

            _log.Info($"Signup success for merchant {model.MerchantName} by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            return Ok();
        }

        [HttpGet]
        [Route("signup/getStarted")]
        public async Task<IActionResult> GetStarted([FromQuery] string email)
        {
            ViewBag.Email = email;
            return View("../User/GetStarted");
        }

        [HttpGet]
        [Route("signup/getStartedConfirmed")]
        public async Task<IActionResult> GetStartedConfirmed()
        {
            return View("../User/GetStartedConfirmed");
        }

        [HttpGet]
        [Route("signup/confirmEmail/{token}")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            _log.Info($"Email confirmation opened with token '{token}' by {HttpContext?.Connection?.RemoteIpAddress?.ToString().SanitizeIp()}");

            try
            {
                var validateTokenResult = ValidateToken(token);

                EmployeeModel employee = null;

                if (validateTokenResult != null)
                {
                    try
                    {
                        employee = await _payInvoiceClient.GetEmployeeAsync(validateTokenResult);
                    }
                    catch (ErrorResponseException e) when (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        _log.Warning($"Invalid token provided: {token}. Employee with {validateTokenResult} is not found.");
                    }
                }

                if (employee == null)
                {
                    _log.Warning($"Invalid token provided: {token}");

                    ModelState.AddModelError(string.Empty, PayInvoicePortalApiErrorCodes.Signup.EmailConfirmation.InvalidToken);

                    return View("../User/ConfirmEmailError");
                }

                await _payAuthClient.SetEmailConfirmedAsync(new EmailConfirmedRequest
                {
                    Email = employee.Email
                });

                _log.Info($"Email confirmation successfully finished with token '{token}'");

                return RedirectToAction(nameof(GetStartedConfirmed), "Signup");
            }
            catch (Exception e)
            {
                _log.Error(e, $"Token {token}");

                ModelState.AddModelError(string.Empty, PayInvoicePortalApiErrorCodes.UnexpectedError);

                return View("../User/ConfirmEmailError");
            }
        }

        private string ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length != TokenLength)
                return null;

            var value = token.Base64ToString();

            if (!value.IsGuid())
                return null;

            return value;
        }
    }
}
