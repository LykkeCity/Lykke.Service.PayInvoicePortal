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
    [Route("api/settings")]
    public class SettingsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;
        private readonly IMerchantService _merchantService;
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public SettingsController(
            IAssetService assetService,
            ILykkeAssetsResolver lykkeAssetsResolver,
            IMerchantService merchantService,
            IPayMerchantClient payMerchantClient,
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient,
            ILogFactory logFactory)
        {
            _assetService = assetService;
            _lykkeAssetsResolver = lykkeAssetsResolver;
            _merchantService = merchantService;
            _payMerchantClient = payMerchantClient;
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [ProducesResponseType(typeof(SettingsResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetSettings()
        {
            var employeeId = User.GetEmployeeId();
            var merchantId = User.GetMerchantId();

            try
            {
                var merchant = await _payMerchantClient.Api.GetByIdAsync(merchantId);

                var publicKeyInfo = await _payAuthClient.GetPayAuthInformationAsync(merchantId);

                var baseAssetId = await _assetService.GetBaseAssetId(merchantId);

                var availableBaseAssets = (await _assetService.GetSettlementAssetsAsync(merchantId)).ToDictionary(_ => _.Key, _ => _.Value);

                // need to add base asset if not empty to the list of available assets
                if (!string.IsNullOrEmpty(baseAssetId) && !availableBaseAssets.ContainsKey(baseAssetId))
                {
                    Asset asset = await _lykkeAssetsResolver.TryGetAssetAsync(baseAssetId);

                    availableBaseAssets.TryAdd(baseAssetId, asset?.DisplayId ?? baseAssetId);
                }

                var settings = new SettingsResponse
                {
                    MerchantDisplayName = merchant.DisplayName,
                    EmployeeFullname = User.GetName(),
                    EmployeeEmail = User.GetEmail(),
                    AvailableBaseAssets = availableBaseAssets.Select(o => new AssetItemViewModel(o.Key, o.Value)).ToList().OrderBy(_ => _.Title),
                    BaseAssetId = baseAssetId,
                    MerchantId = merchantId,
                    MerchantApiKey = merchant.ApiKey,
                    HasPublicKey = !string.IsNullOrEmpty(publicKeyInfo.RsaPublicKey)
                };

                return Ok(settings);
            }
            catch (Exception e)
            {
                _log.Error(e, new { employeeId, merchantId }.ToJson());

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }
        }

        [HttpPost]
        [Route("generateRsaKeys")]
        [ProducesResponseType(typeof(RsaPrivateKeyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
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
                _log.Error(e, new { merchantId }.ToJson());

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }
        }

        [HttpPost]
        [Route("baseAsset")]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetBaseAsset([FromBody] SetBaseAssetModel model)
        {
            var merchantId = User.GetMerchantId();

            try
            {
                await _payInvoiceClient.SetMerchantSettingAsync(new MerchantSetting
                {
                    MerchantId = merchantId,
                    BaseAsset = model.BaseAssetId
                });

                return Ok();
            }
            catch (Exception e)
            {
                _log.Error(e, new { merchantId, model.BaseAssetId }.ToJson());

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }
        }

        [HttpDelete]
        [Route("delete")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete()
        {
            var merchantId = User.GetMerchantId();
            var employeeId = User.GetEmployeeId();

            try
            {
                await _payInvoiceClient.MarkEmployeeDeletedAsync(new MarkEmployeeDeletedRequest
                {
                    MerchantId = merchantId,
                    EmployeeId = employeeId
                });
            }
            catch (Exception e)
            {
                _log.Error(e, new { merchantId, employeeId }.ToJson());

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return NoContent();
        }
    }
}
