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
using Lykke.Pay.Invoice.AppCode;
using Lykke.Pay.Invoice.Models;
using Lykke.Pay.Service.Invoces.Client;
using Lykke.Pay.Service.Invoces.Client.Models;
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
            return NotFound();
        }
        [Route("{InvoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            var respInv = await _invoicesservice.ApiInvoicesByInvoiceIdGetWithHttpMessagesAsync(invoiceId);
            var inv = respInv.Body;
            if (inv == null || !InvoiceStatus.Unpaid.ToString().Equals(inv.Status, StringComparison.InvariantCultureIgnoreCase))
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(inv.StartDate) && inv.StartDate.GetRepoDateTime() > DateTime.Today)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(inv.DueDate) && inv.DueDate.GetRepoDateTime() < DateTime.Now)
            {
                inv.Status = InvoiceStatus.Decline.ToString();
                await _invoicesservice.ApiInvoicesPostWithHttpMessagesAsync(inv.CreateInvoiceEntity());
                return NotFound();
            }

            if (!string.IsNullOrEmpty(inv.WalletAddress))
            {
                return View(await GenerateIfExists(inv, inv.WalletAddress));
            }

            var model = new InvoiceResult
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
            var strToSign = string.Format("{0}{1}", MerchantApiKey, bodyRequest);


            var csp = CreateRsaFromPrivateKey(MerchantPrivateKey);//certificate.GetRSAPrivateKey();
            var sign = Convert.ToBase64String(csp.SignData(Encoding.UTF8.GetBytes(strToSign), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Lykke-Merchant-Id", MerchantId);
            httpClient.DefaultRequestHeaders.Add("Lykke-Merchant-Sign", sign);
            

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
            await _invoicesservice.ApiInvoicesPostWithHttpMessagesAsync(inv.CreateInvoiceEntity());

            model.Amount = orderResp.amount;
            model.QRCode =
                $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:{orderResp.address}?amount={orderResp.amount}%26label=LykkePay%26message={orderResp.orderId}";


            FillViewBag(inv, orderResp);
            
            return View(model);

        }

        private void FillViewBag(IInvoiceEntity inv, dynamic orderResp)
        {
            var orderTimeLive = orderResp.TransactionWaitingTime as string;
            ViewBag.invoiceTimeRefresh =  string.IsNullOrEmpty(orderTimeLive) ? OrderLiveTime.TotalSeconds : (orderTimeLive.FromUnixFormat() - DateTime.Now).TotalSeconds;
            ViewBag.invoiceTimeDueDate = (inv.DueDate.GetRepoDateTime() - DateTime.Now).TotalSeconds;
            ViewBag.orderRequestId = orderResp.orderRequestId;
            ViewBag.invoiceId = inv.InvoiceId;
            ViewBag.invoiceTimeRefresh = (int)Math.Round(ViewBag.invoiceTimeRefresh);
            ViewBag.invoiceTimeDueDate = (int)Math.Round(ViewBag.invoiceTimeDueDate);

        }

        [HttpPost("Regenerate")]
        public async Task<IActionResult> Regenerate(string invoiceId, string orderRequestId)
        {
            
            var respInv = await _invoicesservice.ApiInvoicesByInvoiceIdGetWithHttpMessagesAsync(invoiceId);
            var inv = respInv.Body;
            if (inv == null || !InvoiceStatus.Unpaid.ToString().Equals(inv.Status,StringComparison.InvariantCultureIgnoreCase))
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(inv.StartDate) && inv.StartDate.GetRepoDateTime() > DateTime.Today)
            {
                return NotFound();
            }

            if (inv.DueDate.GetRepoDateTime() < DateTime.Now)
            {
                inv.Status = InvoiceStatus.Decline.ToString();
                await _invoicesservice.ApiInvoicesPostWithHttpMessagesAsync(inv.CreateInvoiceEntity());
                return NotFound();
            }

            var order = await GenerateIfExists(inv, orderRequestId);
            if (order == null)
            {
                return NotFound();
            }

            return Json(order);

        }


        private async Task<InvoiceResult> GenerateIfExists(Service.Invoces.Client.Models.IInvoiceEntity inv, string orderRequestId)
        {
            var model = new InvoiceResult();
           


            model.OrigAmount = inv.Amount;
            model.Currency = inv.Currency;
            model.InvoiceNumber = inv.InvoiceNumber;

            var strToSign = string.Format(MerchantApiKey);


            var csp = CreateRsaFromPrivateKey(MerchantPrivateKey);//certificate.GetRSAPrivateKey();
            var sign = Convert.ToBase64String(csp.SignData(Encoding.UTF8.GetBytes(strToSign), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Lykke-Merchant-Id", MerchantId);
            httpClient.DefaultRequestHeaders.Add("Lykke-Merchant-Sign", sign);


            var result = await httpClient.PostAsync(LykkePayUrl + $"Order/ReCreate/{orderRequestId}",
                new StringContent("", Encoding.UTF8, "application/json"));
            var resp = await result.Content.ReadAsStringAsync();



            if (result.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            dynamic orderResp = JsonConvert.DeserializeObject(resp);

            var paimentRequest = Enum.Parse<MerchantPayRequestStatus>(orderResp.MerchantPayRequestStatus);
            if (paimentRequest == MerchantPayRequestStatus.Completed || paimentRequest == MerchantPayRequestStatus.Failed)
            {
                inv.Status = (paimentRequest == MerchantPayRequestStatus.Completed ? InvoiceStatus.Paid : InvoiceStatus.Decline).ToString();
                await _invoicesservice.ApiInvoicesPostWithHttpMessagesAsync(inv.CreateInvoiceEntity());
                //TODO Add Transaction if success.
                return null;

            }

            model.Amount = orderResp.amount;
            model.QRCode =
                $@"https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:{orderResp.address}?amount={orderResp.amount}%26label=LykkePay%26message={orderResp.orderId}";

            FillViewBag(inv, orderResp);
            return model;
        }


        private static RSA CreateRsaFromPrivateKey(string privateKey)
        {
            privateKey = privateKey.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "");
            var privateKeyBits = System.Convert.FromBase64String(privateKey);


            var RSAparams = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }


            return RSA.Create(RSAparams);
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
            if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

       
    }
}
