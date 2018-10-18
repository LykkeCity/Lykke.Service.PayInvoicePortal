using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Constants;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models.Assets;
using Lykke.Service.PayInvoicePortal.Models.Refund;
using Lykke.Service.PayInvoicePortal.Services.Extensions;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/refund")]
    public class RefundController : Controller
    {
        private readonly ILog _log;
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;

        public RefundController(
            IPayInternalClient payInternalClient,
            ILykkeAssetsResolver lykkeAssetsResolver,
            ILogFactory logFactory)
        {
            _payInternalClient = payInternalClient;
            _lykkeAssetsResolver = lykkeAssetsResolver;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [Route("{paymentRequestId}")]
        [ValidateModel]
        [ProducesResponseType(typeof(RefundDataResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetRefundData([Guid] string paymentRequestId)
        {
            try
            {
                PaymentRequestDetailsModel paymentRequestDetails = await _payInternalClient.GetPaymentRequestDetailsAsync(User.GetMerchantId(), paymentRequestId);

                if (!IsStatusValidForRefund(paymentRequestDetails.Status, paymentRequestDetails.ProcessingError))
                {
                    _log.Warning("Attempt to refund in incorrect status, details: " + paymentRequestDetails.ToJson());

                    return BadRequest(ErrorResponse.Create(RefundErrorType.NotAllowedInStatus.ToString()));
                }

                var paymentAsset = await _lykkeAssetsResolver.TryGetAssetAsync(paymentRequestDetails.PaymentAssetId);

                var refundData = new RefundDataResponse
                {
                    PaymentAsset = Mapper.Map<AssetModel>(paymentAsset),
                    SourceWalletAddresses = paymentRequestDetails.Transactions.SelectMany(x => x.SourceWalletAddresses).ToList(),
                    Amount = paymentRequestDetails.Transactions.Sum(x => x.Amount)
                };

                return Ok(refundData);
            }
            catch (Exception ex)
            {
                _log.ErrorWithDetails(ex, details: new { paymentRequestId });

                return BadRequest(ErrorResponse.Create(PayInvoicePortalApiErrorCodes.UnexpectedError));
            }

            bool IsStatusValidForRefund(PaymentRequestStatus status, PaymentRequestProcessingError processingError)
            {
                var invoiceStatus = status.ToInvoiceStatus(processingError);

                switch (invoiceStatus)
                {
                    case InvoiceStatus.Underpaid:
                    case InvoiceStatus.Overpaid:
                    case InvoiceStatus.LatePaid:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Request refund for certain payment request
        /// </summary>
        /// <param name="model">Refund request model</param>
        /// <returns>204 if ok or 400 with code of error if any occured</returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Refund([FromBody] RefundRequest model)
        {
            try
            {
                _log.Info("Refund requested, request details: " + model.ToJson());
                
                RefundResponse response = await _payInternalClient.RefundAsync(new RefundRequestModel()
                {
                    MerchantId = User.GetMerchantId(),
                    PaymentRequestId = model.PaymentRequestId,
                    DestinationAddress = model.DestinationAddress
                });

                _log.Info("Refund successfully requested, response details: " + response.ToJson());

                return NoContent();
            }
            catch (RefundErrorResponseException ex)
            {
                _log.ErrorWithDetails(ex, details: new { errorCode = ex.Error?.Code.ToString(), model });

                return BadRequest(ErrorResponse.Create(ex.Error?.Code.ToString()));
            }
        }
    }
}

