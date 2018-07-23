using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.Service.PayInvoicePortal.Models.Invoice;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.PayInvoicePortal.Models;

namespace Lykke.Service.PayInvoicePortal.Controllers
{
    [Route("invoice")]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(
            IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        
        [HttpGet]
        [Route("{InvoiceId}")]
        public async Task<IActionResult> Index(string invoiceId)
        {
            PaymentDetails paymentDetails = await _invoiceService.GetPaymentDetailsAsync(invoiceId, force: false);
            IReadOnlyList<FileInfoModel> files = await _invoiceService.GetFilesAsync(invoiceId);

            if (paymentDetails == null)
                return NotFound();

            var model = Mapper.Map<PaymentDetailsModel>(paymentDetails);
            model.Files = Mapper.Map<List<FileModel>>(files);

            var vm = new InvoiceViewModel
            {
                PaymentDetails = model
            };

            return View(vm);
        }
    }
}
