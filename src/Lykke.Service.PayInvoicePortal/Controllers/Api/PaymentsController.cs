using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models.Payments;
using Lykke.Service.PayInvoicePortal.Services.Extensions;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsResponse = Lykke.Service.PayInvoicePortal.Models.Payments.PaymentsResponse;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/payments")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentsService _paymentsService;
        private readonly ILog _log;

        public PaymentsController(
            IPaymentsService paymentsService,
            ILogFactory logFactory
        )
        {
            _paymentsService = paymentsService;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        public async Task<IActionResult> GetByPaymentsFilter(
            PaymentType type,
            PaymentsFilterPeriod period,
            List<PayInvoice.Client.Models.Invoice.InvoiceStatus> statuses,
            string searchText,
            int take)
        {
            var paymentsResponse = await _paymentsService.GetByPaymentsFilter(
                User.GetMerchantId(),
                type,
                statuses,
                period,
                searchText,
                take);
            
            return Ok(Mapper.Map<PaymentsResponse>(paymentsResponse));
        }

        [HttpGet]
        [Route("byInvoiceId/{invoiceId}")]
        [ValidateModel]
        public async Task<IActionResult> GetByInvoiceIdAsync(
            [FromRoute] [Guid] string invoiceId,
            [FromQuery] PaymentsFilterPeriod period,
            [FromQuery] string searchText)
        {
            try
            {
                var model = await _paymentsService.GetByInvoiceIdAsync(invoiceId);

                // check whether satisfy filter conditions
                var dates = period.GetDates();

                if (dates.DateFrom.HasValue &&
                    model.CreatedDate < dates.DateFrom)
                {
                    model = null;
                } 
                else if (dates.DateTo.HasValue &&
                    model.CreatedDate > dates.DateTo)
                {
                    model = null;
                } 
                else if (!string.IsNullOrEmpty(searchText) &&
                    !(model.Number.Contains(searchText) ||
                      model.ClientName.Contains(searchText) ||
                      model.ClientEmail.Contains(searchText)))
                {
                    model = null;
                }

                return Ok(Mapper.Map<PaymentModel>(model));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }
    }
}

