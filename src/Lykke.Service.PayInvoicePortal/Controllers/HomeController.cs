using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using Lykke.Service.PayInvoicePortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using Common;
using Common.Log;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Balances;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Models.Home;
using Microsoft.AspNetCore.Http;
using NewInvoiceModel = Lykke.Service.PayInvoicePortal.Models.NewInvoiceModel;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    [Route("home")]
    public class HomeController : BaseController
    {
        private const string AssetId = "CHF";
        private const string ExchangeAssetId = "BTC";
        
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public HomeController(
            IPayInvoiceClient payInvoiceClient,
            ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _log = log;
        }

        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            var generateditem = TempData["GeneratedItem"];
            if (generateditem != null)
                ViewBag.GeneratedItem = generateditem;
            return View();
        }

        [HttpPost("Profile")]
        public async Task<IActionResult> Profile(NewInvoiceModel model, IFormFile upload)
        {
            InvoiceModel invoice;

            try
            {
                if (model.Status == "Unpaid")
                {
                    if (string.IsNullOrEmpty(model.InvoiceNumber) || string.IsNullOrEmpty(model.ClientEmail) || string.IsNullOrEmpty(model.Amount))
                    {
                        return View();
                    }

                    invoice = await _payInvoiceClient.CreateInvoiceAsync(MerchantId, new CreateInvoiceModel
                    {
                        EmployeeId = EmployeeId,
                        Number = model.InvoiceNumber,
                        ClientName = model.ClientName,
                        ClientEmail = model.ClientEmail,
                        Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                        SettlementAssetId = AssetId,
                        PaymentAssetId = ExchangeAssetId,
                        DueDate = DateTime.Parse(model.StartDate, CultureInfo.InvariantCulture)
                            .Add(Startup.OrderLiveTime)
                    });
                }
                else if (model.Status == "Draft")
                {
                    invoice = await _payInvoiceClient.CreateDraftInvoiceAsync(MerchantId, new CreateDraftInvoiceModel
                    {
                        EmployeeId = EmployeeId,
                        Number = model.InvoiceNumber,
                        ClientName = model.ClientName,
                        ClientEmail = model.ClientEmail,
                        Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                        SettlementAssetId = AssetId,
                        PaymentAssetId = ExchangeAssetId,
                        DueDate = DateTime.Parse(model.StartDate, CultureInfo.InvariantCulture)
                            .Add(Startup.OrderLiveTime)
                    });
                }
                else
                    throw new InvalidOperationException("Unknown action");

                if (upload != null)
                {
                    byte[] content;

                    using (var ms = new MemoryStream())
                    {
                        upload.CopyTo(ms);
                        content = ms.ToArray();
                    }
                    
                    await _payInvoiceClient.UploadFileAsync(invoice.Id, content, upload.FileName, upload.ContentType);
                }
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(HomeController), nameof(Profile), model.ToJson(), exception);
                return BadRequest("Cannot create invoce!");
            }
            
            ViewBag.IsInvoiceCreated = true;
            TempData["GeneratedItem"] = JsonConvert.SerializeObject(invoice);
            return View();
        }

        [HttpPost("Balance")]
        public async Task<JsonResult> Balance()
        {
            double amount = 0d;

            try
            {
                BalanceModel balance = await _payInvoiceClient.GetBalanceAsync(MerchantId, AssetId);

                if (balance?.Balance != null)
                    amount = balance.Balance.Value;
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(HomeController), nameof(Balance), MerchantId, exception);
            }

            return Json($"{amount:0.00}  {AssetId}");
        }

        [HttpGet("InvoiceDetail")]
        public async Task<IActionResult> InvoiceDetail(string invoiceId)
        {
            var model = new InvoiceDetailModel
            {
                Data = await _payInvoiceClient.GetInvoiceAsync(MerchantId, invoiceId)
            };
            var files = await _payInvoiceClient.GetFilesAsync(model.Data.Id);
            model.Files = files.Select(i => new FileModel(i)).ToList();
            if (model.Data.Status != InvoiceStatus.Paid)
            {
                model.InvoiceUrl = $"{SiteUrl.TrimEnd('/')}/invoice/{model.Data.Id}";
                model.QRCode = $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl={model.InvoiceUrl}";
            }

            return View(model);
        }

        [HttpGet("InvoiceFile")]
        public async Task<IActionResult> InvoiceFile(string invoiceId, string fileId, string fileName)
        {
            try
            {
                var stream = await _payInvoiceClient.GetFileAsync(invoiceId, fileId);
                var response = File(stream, "application/octet-stream", fileName);
                return response;
            }
            catch (Exception e)
            {
                // TODO: Loging
                throw new InvalidOperationException("Unknown action");
            }
        }
        [HttpPost("InvoiceDetail")]
        public async Task<IActionResult> InvoiceDetail(InvoiceDetailModel model, IFormFile upload)
        {
            try
            {
                if (model.Data.Status == InvoiceStatus.Unpaid)
                {
                    if (string.IsNullOrEmpty(model.Data.Number) || string.IsNullOrEmpty(model.Data.ClientEmail) || model.Data.Amount < .1m)
                    {
                        // TODO: Need to change invoice process model
                        return RedirectToAction("Profile");
                    }

                    await _payInvoiceClient.CreateInvoiceFromDraftAsync(MerchantId,model.Data.Id, new CreateInvoiceModel
                    {
                        Number = model.Data.Number,
                        ClientName = model.Data.ClientName,
                        ClientEmail = model.Data.ClientEmail,
                        Amount = model.Data.Amount,
                        SettlementAssetId = AssetId,
                        PaymentAssetId = ExchangeAssetId,
                        DueDate = model.Data.DueDate
                    });
                } else if (model.Data.Status == InvoiceStatus.Draft)
                {
                    await _payInvoiceClient.UpdateDraftInvoiceAsync(MerchantId,model.Data.Id, new CreateDraftInvoiceModel
                    {
                        Number = model.Data.Number,
                        ClientName = model.Data.ClientName,
                        ClientEmail = model.Data.ClientEmail,
                        Amount = model.Data.Amount,
                        SettlementAssetId = AssetId,
                        PaymentAssetId = ExchangeAssetId,
                        DueDate = model.Data.DueDate
                    });
                }
                else if (model.Data.Status == InvoiceStatus.Removed)
                {
                    await _payInvoiceClient.DeleteInvoiceAsync(MerchantId, model.Data.Id);
                    return RedirectToAction("Profile");
                }
                else
                {
                    throw new InvalidOperationException("Unknown action");
                }

                if (upload != null)
                {
                    byte[] content;

                    using (var ms = new MemoryStream())
                    {
                        upload.CopyTo(ms);
                        content = ms.ToArray();
                    }
                    
                    await _payInvoiceClient.UploadFileAsync(model.Data.Id, content, upload.FileName, upload.ContentType);
                }
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(HomeController), nameof(InvoiceDetail), model.ToJson(), exception);
                return BadRequest("Error processing invoce!");
            }

            return RedirectToAction("InvoiceDetail", new
            {
                invoiceId = model.Data.Id
            });
        }

        [HttpPost("Invoices")]
        public async Task<JsonResult> Invoices(GridModel model)
        {
            var respmodel = new GridModel();
            IEnumerable<InvoiceModel> invoices = await _payInvoiceClient.GetInvoicesAsync(MerchantId);
            var orderedlist = invoices.OrderByDescending(i => i.CreatedDate).ToList();

            if (model.Filter.Status != "All")
            {
                orderedlist = orderedlist.Where(i => i.Status.ToString() == model.Filter.Status).ToList();
            }

            if (!string.IsNullOrEmpty(model.Filter.SearchValue))
            {
                orderedlist = orderedlist.Where(i =>
                        i.ClientEmail != null && i.ClientEmail.Contains(model.Filter.SearchValue)
                        ||
                        i.Number != null && i.Number.Contains(model.Filter.SearchValue))
                    .OrderByDescending(i => i.CreatedDate)
                    .ToList();
            }

            respmodel.Filter.Status = model.Filter.Status;
            if (!string.IsNullOrEmpty(model.Filter.SortField))
            {
                switch (model.Filter.SortField)
                {
                    case "number":
                        orderedlist = model.Filter.SortWay == 0
                            ? orderedlist.OrderBy(i => i.Number).ThenByDescending(i => i.CreatedDate).ToList()
                            : orderedlist.OrderByDescending(i => i.Number).ThenByDescending(i => i.CreatedDate)
                                .ToList();
                        break;
                    case "client":
                        orderedlist = model.Filter.SortWay == 0
                            ? orderedlist.OrderBy(i => i.ClientName).ThenByDescending(i => i.CreatedDate).ToList()
                            : orderedlist.OrderByDescending(i => i.ClientName).ThenByDescending(i => i.CreatedDate)
                                .ToList();
                        break;
                    case "amount":
                        orderedlist = model.Filter.SortWay == 0
                            ? orderedlist.OrderBy(i => i.Amount).ThenByDescending(i => i.CreatedDate).ToList()
                            : orderedlist.OrderByDescending(i => i.Amount).ThenByDescending(i => i.CreatedDate).ToList();
                        break;
                    case "currency":
                        orderedlist = model.Filter.SortWay == 0
                            ? orderedlist.OrderBy(i => i.SettlementAssetId).ThenByDescending(i => i.CreatedDate).ToList()
                            : orderedlist.OrderByDescending(i => i.SettlementAssetId).ThenByDescending(i => i.CreatedDate)
                                .ToList();
                        break;
                    case "status":
                        orderedlist = model.Filter.SortWay == 0
                            ? orderedlist.OrderBy(i => i.Status).ThenByDescending(i => i.CreatedDate).ToList()
                            : orderedlist.OrderByDescending(i => i.Status).ThenByDescending(i => i.CreatedDate).ToList();
                        break;
                    case "duedate":
                        orderedlist = model.Filter.SortWay == 0
                            ? orderedlist.OrderBy(i => i.DueDate).ToList()
                            : orderedlist.OrderByDescending(i => i.DueDate).ToList();
                        break;
                }
            }
            var period = DateTime.Now;
            var day = period.Day - 1;
            period = period.AddDays(-day).SetTime(0, 0, 0);
            switch (model.Filter.Period)
            {
                case 1:
                    orderedlist = orderedlist.Where(i => i.DueDate >= period).ToList();
                    break;
                case 2:
                    var end = period.AddDays(-1).SetTime(23, 59, 59);
                    var start = period.AddMonths(-1);
                    orderedlist = orderedlist.Where(i => i.DueDate <= end && i.DueDate >= start).ToList();
                    break;
                case 3:
                    var start3 = period.AddMonths(-3);
                    var end3 = start3.AddMonths(1).AddDays(-1).SetTime(23, 59, 59);
                    orderedlist = orderedlist.Where(i => i.DueDate <= end3 && i.DueDate >= start3).ToList();
                    break;
            }

            if (model.Filter.Status == "All")
            {
                respmodel.Header.AllCount = orderedlist.Count;
                respmodel.Header.DraftCount = orderedlist.Count(i => i.Status == InvoiceStatus.Draft);
                respmodel.Header.PaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.Paid);
                respmodel.Header.UnpaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.Unpaid);
                respmodel.Header.RemovedCount = orderedlist.Count(i => i.Status == InvoiceStatus.Removed);
                respmodel.Header.InProgressCount =
                    orderedlist.Count(i => i.Status == InvoiceStatus.InProgress);
                respmodel.Header.LatePaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.LatePaid);
                respmodel.Header.UnderpaidCount =
                    orderedlist.Count(i => i.Status == InvoiceStatus.Underpaid);
                respmodel.Header.OverpaidCount = orderedlist.Count(i => i.Status == InvoiceStatus.Overpaid);
            }

            const int pageSize = 10;
            respmodel.PageCount = (int)Math.Ceiling(orderedlist.Count / (double) pageSize);
            respmodel.Data = orderedlist.Skip((model.Page - 1) * pageSize).Take(pageSize)
                .Select(o=> new GridRowItem
                {
                    Id = o.Id,
                    Number = o.Number,
                    ClientEmail = o.ClientEmail,
                    ClientName = o.ClientName,
                    Amount = o.Amount,
                    DueDate = o.DueDate,
                    Status = o.Status.ToString(),
                    SettlementAssetId = o.SettlementAssetId,
                    CreatedDate = o.CreatedDate.ToLocalTime()
                })
                .ToList();
            
            return Json(respmodel);
        }

        [HttpGet("DeleteInvoice")]
        public async Task<EmptyResult> DeleteInvoice(string invoiceId)
        {
            await _payInvoiceClient.DeleteInvoiceAsync(MerchantId, invoiceId);
            return new EmptyResult();
        }
    }
}
