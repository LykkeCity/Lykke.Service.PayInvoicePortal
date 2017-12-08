using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.AzureRepositories.Azure.Tables;
using Lykke.Core;
using Lykke.Pay.Invoice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Lykke.Pay.Invoice.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        private readonly IInvoiceRepository _invoiceRequestRepo;

        private readonly string _connectionStrings;
        private readonly string _lykkePayOrderUrl;
        private readonly string _merchantId;
        private readonly string _merchantApiKey;
        private readonly string _merchantPrivateKey;

        public HomeController(IConfiguration configuration)
        {
            _connectionStrings = configuration.GetValue<string>("ConnectionStrings");
            _lykkePayOrderUrl = configuration.GetValue<string>("LykkePayOrderUrl");
            _merchantId = configuration.GetValue<string>("MerchantId");
            _merchantApiKey = configuration.GetValue<string>("MerchantApiKey");
            _merchantPrivateKey = configuration.GetValue<string>("MerchantPrivateKey");

            _invoiceRequestRepo =
                new InvoiceRepository(new AzureTableStorage<InvoiceEntity>(_connectionStrings, "Invoices", null));
        }
       
        

        [Route("welcome")]
        public IActionResult Welcome()
        {
            return View();
        }

       

       
    }
}

