using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
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
    }
}

