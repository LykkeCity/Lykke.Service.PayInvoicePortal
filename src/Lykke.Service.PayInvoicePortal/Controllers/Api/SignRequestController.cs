using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Service.PayInvoicePortal.Models.SignRequest;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Route("api/signrequest")]
    public class SignRequestController : Controller
    {
        public const string SystemId = "LykkePay";
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;

        public SignRequestController(
            IPayAuthClient payAuthClient,
            IPayInternalClient payInternalClient,
            ILogFactory logFactory)
        {
            _payAuthClient = payAuthClient;
            _payInternalClient = payInternalClient;
            _log = logFactory.CreateLog(this);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> SignRequest([FromBody] SignRequestModel model)
        {
            try
            {
                var merchant = await _payInternalClient.GetMerchantByIdAsync(model.LykkeMerchantId);

                if (model.ApiKey != merchant.ApiKey)
                    throw new InvalidOperationException("Invalid api key for merchant.");

                var rsa = model.RsaPrivateKey.CreateRsa();

                var signedBody = Convert.ToBase64String(
                    rsa.SignData(
                        Encoding.UTF8.GetBytes($"{model.ApiKey}{model.Body}"),
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1));

                // check signed body
                var verifyRequest = new VerifyRequest
                {
                    ClientId = model.LykkeMerchantId,
                    SystemId = SystemId,
                    Signature = signedBody,
                    Text = model.Body
                };

                var verificationResponse = await _payAuthClient.VerifyAsync(verifyRequest);

                if (!verificationResponse.Description.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException($"Error in verification signed body with response: {verificationResponse.ToJson()}");
                }

                return Ok(new SignRequestResultModel
                {
                    SignedBody = signedBody
                });
            }
            catch (DefaultErrorResponseException ex)
            {
                _log.ErrorWithDetails(ex, model.Sanitize());

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
            catch (Exception ex)
            {
                _log.ErrorWithDetails(ex, model.Sanitize());

                return BadRequest(ErrorResponse.Create("Unable to sign with provided data."));
            }
        }
    }
}
