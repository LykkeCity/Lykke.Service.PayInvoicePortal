using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/export")]
    public class ExportController : Controller
    {
        private readonly IPaymentsService _paymentsService;
        private readonly IMerchantService _merchantService;
        private readonly IInvoiceService _invoiceService;

        public ExportController(
            IPaymentsService paymentsService,
            IMerchantService merchantService,
            IInvoiceService invoiceService)
        {
            _paymentsService = paymentsService;
            _merchantService = merchantService;
            _invoiceService = invoiceService;
        }

        [HttpGet]
        [Route("payments")]
        public async Task<IActionResult> ExportPayments(
            PaymentType type,
            PaymentsFilterPeriod period,
            List<PayInvoice.Client.Models.Invoice.InvoiceStatus> statuses,
            string searchText
        )
        {
            var paymentsResponse = await _paymentsService.GetByPaymentsFilter(
                User.GetMerchantId(),
                type,
                statuses,
                period,
                searchText,
                null
            );

            IEnumerable<Payment> payments = paymentsResponse.Payments;

            string content;

            using (TextWriter stringWriter = new StringWriter())
            {
                var svcWriter = new CsvHelper.CsvWriter(stringWriter);

                svcWriter.WriteRecords(payments.Select(o => new InvoiceCsvRowModel
                {
                    Number = o.Number,
                    ClientName = o.ClientName,
                    ClientEmail = o.ClientEmail,
                    Amount = o.Amount,
                    Currency = o.SettlementAsset.DisplayId,
                    Status = o.Status.ToString(),
                    DueDate = o.DueDate.ToIsoDateTime(),
                    CreatedDate = o.CreatedDate.ToIsoDateTime()
                }));

                content = stringWriter.ToString();
            }

            return File(new MemoryStream(Encoding.UTF8.GetBytes(content)), "text/csv", "payments.csv");
        }

        [HttpGet]
        [Route("supervising")]
        public async Task<IActionResult> GetSupervisingAsync(
            string searchValue,
            Period period,
            List<InvoiceStatus> status,
            string sortField,
            bool sortAscending)
        {
            InvoiceSource source = await _invoiceService.GetSupervisingAsync(
                User.GetMerchantId(),
                User.GetEmployeeId(),
                status,
                period,
                searchValue,
                sortField,
                sortAscending,
                0,
                int.MaxValue);

            string content;

            using (TextWriter stringWriter = new StringWriter())
            {
                var svcWriter = new CsvHelper.CsvWriter(stringWriter);

                var records = new List<InvoiceSupervisingCsvRowModel>();

                foreach (var invoice in source.Items)
                {
                    records.Add(new InvoiceSupervisingCsvRowModel
                    {
                        Creator = await _merchantService.GetMerchantNameAsync(invoice.MerchantId),
                        Number = invoice.Number,
                        ClientName = invoice.ClientName,
                        ClientEmail = invoice.ClientEmail,
                        Amount = invoice.Amount,
                        Currency = invoice.SettlementAsset.DisplayId,
                        Status = invoice.Status.ToString(),
                        DueDate = invoice.DueDate.ToIsoDateTime(),
                        CreatedDate = invoice.CreatedDate.ToIsoDateTime()
                    });
                }

                svcWriter.WriteRecords(records);

                content = stringWriter.ToString();
            }

            return File(new MemoryStream(Encoding.UTF8.GetBytes(content)), "text/csv", "invoices.csv");
        }
    }
}
