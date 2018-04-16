using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
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
        private readonly IInvoiceService _invoiceService;
        private readonly ILog _log;

        public ExportController(IInvoiceService invoiceService, ILog log)
        {
            _invoiceService = invoiceService;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(
            string searchValue,
            Period period,
            List<InvoiceStatus> status,
            string sortField,
            bool sortAscending)
        {
            IEnumerable<Invoice> invoices = await _invoiceService.GetAsync(
                User.GetMerchantId(),
                status,
                period,
                searchValue,
                sortField,
                sortAscending);

            string content;

            using (TextWriter stringWriter = new StringWriter())
            {
                var svcWriter = new CsvHelper.CsvWriter(stringWriter);

                svcWriter.WriteRecords(invoices.Select(o => new InvoiceCsvRowModel
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

            return File(new MemoryStream(Encoding.UTF8.GetBytes(content)), "text/csv", "invoices.csv");
        }
    }
}
