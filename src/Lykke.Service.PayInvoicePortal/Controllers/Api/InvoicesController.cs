using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Client.Models;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoice.Contract.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.Models;
using Lykke.Service.PayInvoicePortal.Models.Invoices;
using Lykke.Service.PayInvoicePortal.Models.Invoices.Statistic;
using Lykke.Service.PayInvoicePortal.Services;
using Lykke.Service.PayInvoicePortal.Services.Extensions;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HistoryItemModel = Lykke.Service.PayInvoicePortal.Models.Invoices.HistoryItemModel;
using InvoiceModel = Lykke.Service.PayInvoicePortal.Models.Invoices.InvoiceModel;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Authorize]
    [Route("/api/invoices")]
    public class InvoicesController : Controller
    {
        private readonly IRealtimeNotificationsService _realtimeNotificationsService;
        private readonly IInvoiceService _invoiceService;
        private readonly IAssetService _assetService;
        private readonly ILog _log;

        public InvoicesController(
            IRealtimeNotificationsService realtimeNotificationsService,
            IInvoiceService invoiceService,
            IAssetService assetService,
            ILogFactory logFactory)
        {
            _realtimeNotificationsService = realtimeNotificationsService;
            _invoiceService = invoiceService;
            _assetService = assetService;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [Route("{invoiceId}")]
        [ValidateModel]
        public async Task<IActionResult> GetByIdAsync([Guid] string invoiceId)
        {
            try
            {
                var model = await GetInvoiceModelById(invoiceId);
            
                return Ok(model);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("details/{invoiceId}")]
        [ValidateModel]
        public async Task<IActionResult> GetInvoiceDetails([Guid] string invoiceId)
        {
            try
            {
                var invoice = await GetInvoiceModelById(invoiceId);

                var response = new InvoiceDetailsResponse
                {
                    Invoice = invoice,
                    BlockchainExplorerUrl = Startup.BlockchainExplorerUrl.TrimEnd('/'),
                    EthereumBlockchainExplorerUrl = Startup.EthereumBlockchainExplorerUrl.TrimEnd('/')
                };

                return Ok(response);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }
        
        private async Task<InvoiceModel> GetInvoiceModelById(string invoiceId)
        {
            var merchantId = User.GetMerchantId();

            var invoiceTask = _invoiceService.GetByIdAsync(invoiceId);
            var filesTask = _invoiceService.GetFilesAsync(invoiceId);
            var historyTask = _invoiceService.GetHistoryAsync(merchantId, invoiceId);

            await Task.WhenAll(
                invoiceTask,
                filesTask,
                historyTask
            );

            var invoice = Mapper.Map<InvoiceModel>(invoiceTask.Result);
            invoice.Files = Mapper.Map<List<FileModel>>(filesTask.Result.ToList().OrderBy(x => x.Name));
            invoice.History = Mapper.Map<List<HistoryItemModel>>(historyTask.Result);

            invoice.PaymentAssetNetwork = (await _assetService.GetAssetNetworkAsync(invoice.PaymentAsset)).ToString();

            return invoice;
        }
        
        [HttpGet]
        [Route("/api/invoices/supervising")]
        public async Task<IActionResult> GetSupervisingInvoicesAsync(
            string searchValue,
            Period period,
            List<PayInvoice.Client.Models.Invoice.InvoiceStatus> status,
            string sortField,
            bool sortAscending,
            int skip,
            int take)
        {

            InvoiceSource source = await _invoiceService.GetSupervisingAsync(
                User.GetMerchantId(),
                User.GetEmployeeId(),
                status,
                period,
                searchValue,
                sortField,
                sortAscending,
                skip,
                take);

            var model = new InvoicesGatheredInfoModel
            {
                List = new ListModel
                {
                    Total = source.Total,
                    CountPerStatus = source.CountPerStatus.ToDictionary(o => o.Key.ToString(), o => o.Value),
                    Items = Mapper.Map<List<ListItemModel>>(source.Items)
                },
                Balance = source.Balance,
                BaseAsset = source.BaseAsset,
                BaseAssetAccuracy = source.BaseAssetAccuracy,
                Statistic = new StatisticModel
                {
                    MainStatistic = source.MainStatistic.ToDictionary(x => x.Key.ToString(), x => x.Value),
                    SummaryStatistic = source.SummaryStatistic,
                    Rates = source.Rates,
                    HasErrorsInStatistic = source.HasErrorsInStatistic
                }
            };

            return Json(model);
        }

        [HttpPost]
        [ProducesResponseType(typeof(InvoiceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> AddAsync([Required] CreateInvoiceRequest model, IFormFileCollection files)
        {
            Invoice invoice = null;

            var request = new PayInvoice.Client.Models.Invoice.CreateInvoiceModel
            {
                MerchantId = User.GetMerchantId(),
                EmployeeId = User.GetEmployeeId(),
                Number = model.Number,
                ClientName = model.Client,
                ClientEmail = model.Email,
                Amount = model.Amount,
                SettlementAssetId = model.SettlementAssetId,
                DueDate = model.DueDate,
                Note = model.Note
            };

            try
            {
                invoice = await _invoiceService.CreateAsync(request, model.IsDraft);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorResponse.Create(ex.Message));
            }

            if (files != null)
            {
                foreach (IFormFile formFile in files)
                {
                    byte[] content;

                    using (var ms = new MemoryStream())
                    {
                        formFile.CopyTo(ms);
                        content = ms.ToArray();
                    }

                    await _invoiceService.UploadFileAsync(invoice.Id, content, formFile.FileName, formFile.ContentType);
                }
            }

            IEnumerable<FileInfoModel> invoiceFiles = await _invoiceService.GetFilesAsync(invoice.Id);

            var result = Mapper.Map<InvoiceModel>(invoice);
            result.Files = Mapper.Map<List<FileModel>>(invoiceFiles);

            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromBody]UpdateInvoiceRequest model)
        {
            var invoice = new PayInvoice.Client.Models.Invoice.UpdateInvoiceModel
            {
                Id = model.Id,
                MerchantId = User.GetMerchantId(),
                EmployeeId = User.GetEmployeeId(),
                Number = model.Number,
                ClientName = model.Client,
                ClientEmail = model.Email,
                Amount = model.Amount,
                SettlementAssetId = model.SettlementAssetId,
                DueDate = model.DueDate,
                Note = model.Note
            };

            try
            {
                await _invoiceService.UpdateAsync(invoice, model.IsDraft);

                if (model.IsDraft)
                {
                    await _realtimeNotificationsService.SendInvoiceUpdateAsync(new InvoiceUpdateMessage()
                    {
                        MerchantId = User.GetMerchantId(),
                        InvoiceId = invoice.Id,
                        Status = "DraftUpdated"
                    });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorResponse.Create(ex.Message));
            }

            var result = await GetInvoiceModelById(invoice.Id);

            return Json(result);
        }

        [HttpDelete]
        [Route("{invoiceId}")]
        public async Task<IActionResult> DeleteInvoice(string invoiceId)
        {
            var status = await _invoiceService.GetStatusOnlyAsync(invoiceId);

            await _invoiceService.DeleteAsync(invoiceId);

            if (status == InvoiceStatus.Draft)
            {
                await _realtimeNotificationsService.SendInvoiceUpdateAsync(new InvoiceUpdateMessage()
                {
                    MerchantId = User.GetMerchantId(),
                    InvoiceId = invoiceId,
                    Status = "DraftRemoved"
                });
            }

            return NoContent();
        }
    }
}
