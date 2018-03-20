using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/invoices")]
    public class InvoicesController : BaseController
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IInvoiceService _invoiceService;
        private readonly ILog _log;

        public InvoicesController(IPayInvoiceClient payInvoiceClient, IInvoiceService invoiceService, ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _invoiceService = invoiceService;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvociesAsync(
            string searchValue,
            Period period,
            List<InvoiceStatus> status,
            string sortField,
            bool sortAscending,
            int skip,
            int take)
        {
            InvoiceSource source = await _invoiceService.GetAsync(
                MerchantId,
                status,
                period,
                searchValue,
                sortField,
                sortAscending,
                skip,
                take);

            var model = new InvoiceListModel
            {
                Total = source.Total,
                CountPerStatus = source.CountPerStatus.ToDictionary(o => o.Key.ToString(), o => o.Value),
                Items = source.Items.Select(o => new InvoiceListItemModel
                    {
                        Id = o.Id,
                        Number = o.Number,
                        ClientEmail = o.ClientEmail,
                        ClientName = o.ClientName,
                        Amount = (double)o.Amount,
                        DueDate = o.DueDate,
                        Status = o.Status.ToString(),
                        Currency = o.SettlementAssetId,
                        CreatedDate = o.CreatedDate
                    })
                    .ToList()
            };

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(Models.Invoices.CreateInvoiceModel model, IFormFileCollection files)
        {
            var result = new Models.Invoices.InvoiceModel
            {
                Id = "new",
                Number = "N789",
                ClientEmail = "client@client.com",
                ClientName = "Alex",
                Amount = 23.7,
                DueDate = DateTime.Today.AddDays(1),
                Status = "Unpaid",
                Currency = "CHF",
                CreatedDate = DateTime.Today,
                Files = new List<Models.Invoices.FileModel>
                {
                    new Models.Invoices.FileModel{Id = "asdasd", Name = "Inside Microsoft SharePoint 2016.pdf", Size = 124246},
                    new Models.Invoices.FileModel{Id = "4636", Name = "Smaller Habits, Bigger Results.pdf", Size = 1294246}
                }
            };

            return Json(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(Models.Invoices.CreateInvoiceModel model, IFormFileCollection files)
        {
            var result = new Models.Invoices.InvoiceModel();

            return Json(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteInvoice(string invoiceId)
        {
            await _payInvoiceClient.DeleteInvoiceAsync(MerchantId, invoiceId);
            return NoContent();
        }
    }
}
