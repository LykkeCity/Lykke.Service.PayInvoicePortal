using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.GenerateRsaKeys;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.MerchantSetting;
using Lykke.Service.PayInvoicePortal.Constants;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models.User;
using Lykke.Service.PayMerchant.Client;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("api/settings")]
    public class SettingsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IMerchantService _merchantService;
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public SettingsController(
            IAssetService assetService,
            IMerchantService merchantService,
            IPayMerchantClient payMerchantClient,
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient,
            ILogFactory logFactory)
        {
            _assetService = assetService;
            _merchantService = merchantService;
            _payMerchantClient = payMerchantClient;
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [ProducesResponseType(typeof(SettingsResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetSettings()
        {
            var employeeId = User.GetEmployeeId();
            var merchantId = User.GetMerchantId();

            try
            {
                var merchant = await _payMerchantClient.Api.GetByIdAsync(merchantId);

                //TODO: getting public key info
                //var publicKeyInfo = await _payAuthClient.GetById

                //TODO: make autofill to CHF if empty
                var availableBaseAssets = new Dictionary<string, string>();

                var settings = new SettingsResponse
                {
                    MerchantDisplayName = merchant.DisplayName,
                    EmployeeFullname = User.GetName(),
                    EmployeeEmail = User.GetEmail(),
                    AvailableBaseAssets = availableBaseAssets,
                    BaseAsset = await _assetService.GetBaseAssetId(merchantId),
                    MerchantId = merchantId,
                    MerchantApiKey = merchant.ApiKey,
                    HasPublicKey = false //TODO
                };

                return Ok(settings);
            }
            catch (Exception e)
            {
                _log.Error(e, new {employeeId,merchantId}.ToJson());

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }
        }

        [HttpPost]
        [Route("generateRsaKeys")]
        [ProducesResponseType(typeof(RsaPrivateKeyResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GenerateRsaKeys()
        {
            var merchantId = User.GetMerchantId();

            try
            {
                var rsaKeysResponse = await _payAuthClient.GenerateRsaKeysAsync(new GenerateRsaKeysRequest
                {
                    ClientId = merchantId,
                    ClientDisplayName = await _merchantService.GetMerchantNameAsync(merchantId)
                });

                return Ok(new RsaPrivateKeyResponse
                {
                    RsaPrivateKey = rsaKeysResponse.PrivateKey
                });
            }
            catch (Exception e)
            {
                _log.Error(e, new {merchantId}.ToJson());
                
                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }
        }

        [HttpPost]
        [Route("baseAsset")]
        [ValidateModel]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetBaseAsset([Required] string baseAsset)
        {
            var merchantId = User.GetMerchantId();

            try
            {
                await _payInvoiceClient.SetBaseAssetAsync(new UpdateBaseAssetRequest
                {
                    MerchantId = merchantId,
                    BaseAsset = baseAsset
                });

                return Ok();
            }
            catch (Exception e)
            {
                _log.Error(e, new {merchantId, baseAsset}.ToJson());
                
                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }
        }

        [HttpPost]
        [Route("delete")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete()
        {
            var merchantId = User.GetMerchantId();
            var employeeId = User.GetEmployeeId();

            try
            {
                //TODO make call to mark deleted
            }
            catch (Exception e)
            {
                _log.Error(e, new {merchantId,employeeId}.ToJson());

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }

            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return Ok();
        }
    }
}
