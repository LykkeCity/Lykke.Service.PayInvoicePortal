using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lykke.Core;
using Lykke.Pay.Common;
using Lykke.Pay.Invoice.AppCode;
using Lykke.Pay.Invoice.Models;
using Lykke.Pay.Service.Invoces.Client;
using Lykke.Pay.Service.Invoces.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using IInvoiceEntity = Lykke.Pay.Service.Invoces.Client.Models.IInvoiceEntity;

namespace Lykke.Pay.Invoice.Controllers
{
    public class InvoiceController : BaseController
    {
        private readonly IInvoicesservice _invoicesservice;

        public InvoiceController(IConfiguration configuration, IInvoicesservice invoicesservice) : base(configuration)
        {
            _invoicesservice = invoicesservice;
        }


        public IActionResult Index()
        {
            return Redirect(HomeUrl);
        }
        [Route("invoice/{InvoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            var respInv = await _invoicesservice.ApiInvoicesByInvoiceIdGetWithHttpMessagesAsync(invoiceId, MerchantId);
            var inv = respInv.Body;
            if (inv == null)
            {
                return NotFound();
            }

            var invoiceStatus = inv.Status.ParsePayEnum<InvoiceStatus>();

            if (invoiceStatus == InvoiceStatus.Draft || invoiceStatus == InvoiceStatus.Removed ||
                !string.IsNullOrEmpty(inv.StartDate) && inv.StartDate.GetRepoDateTime() > DateTime.Now)
            {
                return NotFound();
            }

            InvoiceResult model = null;


            if (!string.IsNullOrEmpty(inv.WalletAddress))
            {
                model = await GenerateIfExists(inv, inv.WalletAddress);
            }



            if (!string.IsNullOrEmpty(inv.DueDate) && inv.DueDate.GetRepoDateTime() < DateTime.Now)
            {
                inv.Status = InvoiceStatus.LatePaid.ToString();
                ViewBag.invoiceStatus = inv.Status;
                await _invoicesservice.ApiInvoicesPostWithHttpMessagesAsync(inv.CreateInvoiceEntity(MerchantId));
            }
            else
            {
                if (model == null)
                {
                    model = new InvoiceResult()
                    {
                        OrigAmount = inv.Amount,
                        Currency = inv.Currency,
                        InvoiceNumber = inv.InvoiceNumber
                    };

                    var order = new
                    {
                        inv.Currency,
                        inv.Amount,
                        ExchangeCurrency = "BTC",
                        OrderId = inv.InvoiceNumber,
                        Markup = new
                        {
                            Percent = 1,
                            Pips = 10
                        }
                    };




                    var bodyRequest = JsonConvert.SerializeObject(order);

                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("Lykke-Merchant-Id", MerchantId);
                    httpClient.DefaultRequestHeaders.Add("Lykke-Merchant-Traster-SignIn", "true");


                    var result = await httpClient.PostAsync(LykkePayUrl + "Order",
                        new StringContent(bodyRequest, Encoding.UTF8, "application/json"));
                    var resp = await result.Content.ReadAsStringAsync();



                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        return BadRequest();
                    }

                    dynamic orderResp = JsonConvert.DeserializeObject(resp);

                    inv.WalletAddress = orderResp.address;
                    inv.DueDate = DateTime.Now.Add(InvoiceLiveTime).RepoDateStr();
                    inv.StartDate = DateTime.Now.RepoDateStr();
                    await _invoicesservice.ApiInvoicesPostWithHttpMessagesAsync(inv.CreateInvoiceEntity(MerchantId));

                    model.Amount = RoundDouble((double)orderResp.amount);
                    model.QRCode =
                        $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:{orderResp.address}?amount={model.Amount}%26label=invoice%20#{inv.InvoiceNumber}%26message={orderResp.orderId}";

                    FillViewBag(inv, orderResp);
                }
            }
            ViewBag.invoiceStatus = inv.Status;

            ViewBag.needAutoUpdate = invoiceStatus == InvoiceStatus.InProgress || invoiceStatus == InvoiceStatus.Unpaid
                ? 1
                : 0;


            return View(model);

        }

        [HttpPost("status")]
        public async Task<JsonResult> Status(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return Json(new { status = InvoiceStatus.Removed });
            }
            var resp = await _invoicesservice.ApiInvoicesGetWithHttpMessagesAsync(MerchantId);
            if (resp.Body == null)
            {
                return Json(new { status = InvoiceStatus.Removed });
            }
            var invoice = (from i in resp.Body
                           where address.Equals(i.WalletAddress, StringComparison.InvariantCultureIgnoreCase)
                           select i).FirstOrDefault();

            if (invoice == null)
            {
                return Json(new { status = InvoiceStatus.Removed });
            }

            return Json(new { status = invoice.Status.ParsePayEnum<InvoiceStatus>() });
        }
        private void FillViewBag(IInvoiceEntity inv, dynamic orderResp)
        {
            string orderTimeLive = orderResp.transactionWaitingTime.ToString();
            ViewBag.invoiceTimeRefresh = string.IsNullOrEmpty(orderTimeLive) ? OrderLiveTime.TotalSeconds : (orderTimeLive.FromUnixFormat() - DateTime.Now).TotalSeconds;
            ViewBag.invoiceTimeDueDate = (inv.DueDate.GetRepoDateTime() - DateTime.Now).TotalSeconds;
            ViewBag.address = orderResp.address;
            ViewBag.invoiceId = inv.InvoiceId;
            ViewBag.invoiceTimeRefresh = (int)Math.Round(ViewBag.invoiceTimeRefresh);
            ViewBag.invoiceTimeDueDate = (int)Math.Round(ViewBag.invoiceTimeDueDate);

        }

        [HttpPost("Regenerate")]
        public async Task<IActionResult> Regenerate(string invoiceId, string address)
        {

            var respInv = await _invoicesservice.ApiInvoicesByInvoiceIdGetWithHttpMessagesAsync(invoiceId, MerchantId);
            var inv = respInv.Body;
            if (inv == null || !InvoiceStatus.Unpaid.ToString().Equals(inv.Status, StringComparison.InvariantCultureIgnoreCase))
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(inv.StartDate) && inv.StartDate.GetRepoDateTime() > DateTime.Today)
            {
                return NotFound();
            }

            if (inv.DueDate.GetRepoDateTime() < DateTime.Now)
            {
                inv.Status = InvoiceStatus.Removed.ToString();
                await _invoicesservice.ApiInvoicesPostWithHttpMessagesAsync(inv.CreateInvoiceEntity(MerchantId));
                return NotFound();
            }

            var order = await GenerateIfExists(inv, address);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.invoiceStatus = inv.Status;

            var invoiceStatus = inv.Status.ParsePayEnum<InvoiceStatus>();

            return Json(new
            {
                order,
                ViewBag.invoiceTimeRefresh,
                status = inv.Status,
                needAutoUpdate = invoiceStatus == InvoiceStatus.InProgress || invoiceStatus == InvoiceStatus.Unpaid
                    ? 1
                    : 0
            });

        }


        private async Task<InvoiceResult> GenerateIfExists(Service.Invoces.Client.Models.IInvoiceEntity inv, string address)
        {
            var model = new InvoiceResult();



            model.OrigAmount = inv.Amount;
            model.Currency = inv.Currency;
            model.InvoiceNumber = inv.InvoiceNumber;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Lykke-Merchant-Id", MerchantId);
            httpClient.DefaultRequestHeaders.Add("Lykke-Merchant-Sign", "true");


            var result = await httpClient.PostAsync(LykkePayUrl + $"Order/ReCreate/{address}",
                new StringContent("", Encoding.UTF8, "application/json"));
            var resp = await result.Content.ReadAsStringAsync();



            if (result.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            dynamic orderResp = JsonConvert.DeserializeObject(resp);


            var paimentRequest = ((string)orderResp.merchantPayRequestStatus).ParsePayEnum<MerchantPayRequestStatus>();


            if (paimentRequest == MerchantPayRequestStatus.Completed || paimentRequest == MerchantPayRequestStatus.Failed)
            {
                inv.Status = ((string)orderResp.transactionStatus).ParsePayEnum<InvoiceStatus>().ToString();
                await _invoicesservice.ApiInvoicesPostWithHttpMessagesAsync(inv.CreateInvoiceEntity(MerchantId));

            }

            model.Amount = RoundDouble((double)orderResp.amount);
            model.QRCode =
                $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:{orderResp.address}?amount={model.Amount}%26label=LykkePay%26message={orderResp.orderId}";

            FillViewBag(inv, orderResp);
            return model;
        }

        private double RoundDouble(double modelAmount)
        {
            return Math.Round(modelAmount, 8);
        }

        [HttpPost()]
        public async Task<IActionResult> UpdateStatusSuccess()

        #region Sign request methods
        //private static RSA CreateRsaFromPrivateKey(string privateKey)
        //{
        //    privateKey = privateKey.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "");
        //    var privateKeyBits = System.Convert.FromBase64String(privateKey);


        //    var RSAparams = new RSAParameters();

        //    using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
        //    {
        //        byte bt = 0;
        //        ushort twobytes = 0;
        //        twobytes = binr.ReadUInt16();
        //        if (twobytes == 0x8130)
        //            binr.ReadByte();
        //        else if (twobytes == 0x8230)
        //            binr.ReadInt16();
        //        else
        //            throw new Exception("Unexpected value read binr.ReadUInt16()");

        //        twobytes = binr.ReadUInt16();
        //        if (twobytes != 0x0102)
        //            throw new Exception("Unexpected version");

        //        bt = binr.ReadByte();
        //        if (bt != 0x00)
        //            throw new Exception("Unexpected value read binr.ReadByte()");

        //        RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
        //        RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
        //        RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
        //        RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
        //        RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
        //        RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
        //        RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
        //        RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
        //    }


        //    return RSA.Create(RSAparams);
        //}

        //private static int GetIntegerSize(BinaryReader binr)
        //{
        //    byte bt = 0;
        //    byte lowbyte = 0x00;
        //    byte highbyte = 0x00;
        //    int count = 0;
        //    bt = binr.ReadByte();
        //    if (bt != 0x02)
        //        return 0;
        //    bt = binr.ReadByte();

        //    if (bt == 0x81)
        //        count = binr.ReadByte();
        //    else
        //    if (bt == 0x82)
        //    {
        //        highbyte = binr.ReadByte();
        //        lowbyte = binr.ReadByte();
        //        byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
        //        count = BitConverter.ToInt32(modint, 0);
        //    }
        //    else
        //    {
        //        count = bt;
        //    }

        //    while (binr.ReadByte() == 0x00)
        //    {
        //        count -= 1;
        //    }
        //    binr.BaseStream.Seek(-1, SeekOrigin.Current);
        //    return count;
        //}
        #endregion

    }
}
