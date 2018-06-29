using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Domain.Incoming;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Lykke.Service.PayInvoicePortal.Models.Invoices.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/incomingInvoices")]
    public class IncomingInvoicesController : Controller
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IInvoiceService _invoiceService;

        public IncomingInvoicesController(
            IPayInvoiceClient payInvoiceClient,
            IInvoiceService invoiceService)
        {
            _payInvoiceClient = payInvoiceClient;
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetIncomingInvoices(
            string searchValue,
            Period period,
            List<InvoiceStatus> statuses,
            int skip,
            int take)
        {
            IncomingInvoicesSource source = await _invoiceService.GetIncomingAsync(
                User.GetMerchantId(),
                statuses,
                period,
                searchValue,
                skip,
                take);

            return Json(new
            {
                List = new
                {
                    source.Total,
                    source.CountPerStatus,
                    source.Items
                },
                source.BaseAsset
            });
        }

        [HttpGet("sum")]
        public async Task<IActionResult> GetSumToPayInvoices(IEnumerable<string> invoicesIds, string assetForPay)
        {
            decimal result = 0;

            try
            {
                result = await _payInvoiceClient.GetSumToPayInvoicesAsync(new GetSumToPayInvoicesRequest
                {
                    EmployeeId = User.GetEmployeeId(),
                    InvoicesIds = invoicesIds,
                    AssetForPay = assetForPay
                });
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }

            return Accepted(result);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> PayInvoices([FromBody] Models.Invoices.PayInvoicesRequest model)
        {
            try
            {
                await _payInvoiceClient.PayInvoicesAsync(new PayInvoice.Client.Models.Invoice.PayInvoicesRequest
                {
                    EmployeeId = User.GetEmployeeId(),
                    InvoicesIds = model.InvoicesIds,
                    Amount = model.Amount,
                    AssetForPay = model.AssetForPay
                });
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(ex.Error);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ex.Error);
            }

            return Accepted(true);
        }
    }
}
