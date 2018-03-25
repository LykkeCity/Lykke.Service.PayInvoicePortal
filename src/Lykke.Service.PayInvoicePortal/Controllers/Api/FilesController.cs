using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.File;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PayInvoicePortal.Controllers.Api
{
    [Route("/api/files")]
    public class FilesController : Controller
    {
        private readonly IPayInvoiceClient _payInvoiceClient;

        public FilesController(IPayInvoiceClient payInvoiceClient)
        {
            _payInvoiceClient = payInvoiceClient;
        }

        [HttpGet]
        [Route("{fileId}")]
        public async Task<IActionResult> GetFileSync(string fileId, string invoiceId)
        {
            IEnumerable<FileInfoModel> files = await _payInvoiceClient.GetFilesAsync(invoiceId);
            byte[] content = await _payInvoiceClient.GetFileAsync(invoiceId, fileId);

            FileInfoModel file = files.FirstOrDefault(o => o.Id == fileId);

            return File(content, file.Type, file.Name);
        }

        [Authorize]
        [HttpDelete]
        [Route("{fileId}")]
        public async Task<IActionResult> DeleteFile(string fileId, string invoiceId)
        {
            await _payInvoiceClient.DeleteFileAsync(invoiceId, fileId);
            return NoContent();
        }
    }
}
