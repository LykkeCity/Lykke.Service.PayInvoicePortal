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
using Lykke.Service.PayInvoice.Client.Models.Assets;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Models.Home;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
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

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Profile")]
        public async Task<IActionResult> Profile(NewInvoiceModel model, IFormFileCollection upload)
        {
            PayInvoice.Client.Models.Invoice.InvoiceModel invoice;

            try
            {
                if (model.Status == "Unpaid")
                {
                    if (string.IsNullOrEmpty(model.InvoiceNumber) || string.IsNullOrEmpty(model.ClientEmail) || string.IsNullOrEmpty(model.Amount))
                    {
                        return View();
                    }

                    invoice = await _payInvoiceClient.CreateInvoiceAsync(MerchantId, new PayInvoice.Client.Models.Invoice.CreateInvoiceModel
                    {
                        EmployeeId = EmployeeId,
                        Number = model.InvoiceNumber,
                        ClientName = model.ClientName,
                        ClientEmail = model.ClientEmail,
                        Amount = decimal.Parse(model.Amount, CultureInfo.InvariantCulture),
                        SettlementAssetId = model.Currency,
                        PaymentAssetId = ExchangeAssetId,
                        DueDate = DateTime.ParseExact(model.StartDate, "dd.MM.yyyy", CultureInfo.InvariantCulture)
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
                        SettlementAssetId = model.Currency,
                        PaymentAssetId = ExchangeAssetId,
                        DueDate = DateTime.ParseExact(model.StartDate, "dd.MM.yyyy", CultureInfo.InvariantCulture)
                            .Add(Startup.OrderLiveTime)
                    });
                }
                else
                    throw new InvalidOperationException("Unknown action");
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(HomeController), nameof(Profile), model.ToJson(), exception);
                return BadRequest("Cannot create invoce!");
            }

            if (upload != null)
            {
                foreach (IFormFile formFile in upload)
                {
                    byte[] content;

                    using (var ms = new MemoryStream())
                    {
                        formFile.CopyTo(ms);
                        content = ms.ToArray();
                    }

                    await _payInvoiceClient.UploadFileAsync(invoice.Id, content, formFile.FileName, formFile.ContentType);
                }
            }

            var viewModel = new InvoiceViewModel
            {
                Id = invoice.Id,
                Number = invoice.Number,
                ClientName = invoice.ClientName,
                ClientEmail = invoice.ClientEmail,
                Amount = $"{invoice.Amount:N2}  {invoice.SettlementAssetId}",
                DueDate = invoice.DueDate.ToString("MM/dd/yyyy"),
                Status = invoice.Status.ToString(),
                SettlementAssetId = invoice.SettlementAssetId,
                CreatedDate = invoice.CreatedDate.ToString("MM/dd/yyyy")
            };

            IReadOnlyList<AssetModel> assets = await _payInvoiceClient.GetSettlementAssetsAsync();

            ViewBag.Assets  = assets
                .Select(o => new ItemViewModel(o.Id, o.Name))
                .ToList();

            ViewBag.IsInvoiceCreated = true;
            TempData["GeneratedItem"] = JsonConvert.SerializeObject(viewModel);
            return View();
        }

        [HttpGet("InvoiceDetail")]
        public async Task<IActionResult> InvoiceDetail(string invoiceId)
        {
            var model = new InvoiceDetailModel
            {
                Data = await _payInvoiceClient.GetInvoiceAsync(MerchantId, invoiceId)
            };

            IEnumerable<FileInfoModel> files = await _payInvoiceClient.GetFilesAsync(model.Data.Id);

            model.Files = files
                .Select(o => new Models.FileModel
                {
                    FileName = o.Name,
                    FileExtension = Path.GetExtension(o.Name).TrimStart('.'),
                    FileUrl = Url.Action("InvoiceFile", new
                    {
                        invoiceId,
                        o.Id,
                        o.Name,
                        o.Type
                    }),
                    FileSize = o.Size < 1024
                        ? $"{o.Size} bytes"
                        : o.Size > 1024 && o.Size < 1048576
                            ? $"{o.Size / 1024:N0} KB"
                            : $"{o.Size / 1048576:N0} MB"
                })
                .ToList();

            if (model.Data.Status != InvoiceStatus.Paid)
            {
                model.InvoiceUrl = $"{SiteUrl.TrimEnd('/')}/invoice/{model.Data.Id}";
                model.QRCode = $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl={model.InvoiceUrl}";
            }

            if (model.Data.Status == InvoiceStatus.Paid)
            {
                model.BlockchainExplorerUrl = $"{BlockchainExplorerUrl.TrimEnd('/')}/address/{model.Data.WalletAddress}";
            }

            IReadOnlyList<AssetModel> assets = await _payInvoiceClient.GetSettlementAssetsAsync();

            ViewBag.Assets  = assets
                .Select(o => new ItemViewModel(o.Id, o.Name))
                .ToList();

            return View(model);
        }

        [HttpPost("InvoiceDetail")]
        public async Task<IActionResult> InvoiceDetail(InvoiceDetailModel model, IFormFileCollection upload)
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

                    await _payInvoiceClient.CreateInvoiceFromDraftAsync(MerchantId, model.Data.Id, new PayInvoice.Client.Models.Invoice.CreateInvoiceModel
                    {
                        Number = model.Data.Number,
                        ClientName = model.Data.ClientName,
                        ClientEmail = model.Data.ClientEmail,
                        Amount = model.Data.Amount,
                        SettlementAssetId = model.Data.SettlementAssetId,
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
                        SettlementAssetId = model.Data.SettlementAssetId,
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
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(HomeController), nameof(InvoiceDetail), model.ToJson(), exception);
                return BadRequest("Error processing invoce!");
            }

            if (upload != null)
            {
                foreach (IFormFile formFile in upload)
                {
                    byte[] content;

                    using (var ms = new MemoryStream())
                    {
                        formFile.CopyTo(ms);
                        content = ms.ToArray();
                    }

                    await _payInvoiceClient.UploadFileAsync(model.Data.Id, content, formFile.FileName, formFile.ContentType);
                }
            }

            return RedirectToAction("InvoiceDetail", new
            {
                invoiceId = model.Data.Id
            });
        }
    }
}
