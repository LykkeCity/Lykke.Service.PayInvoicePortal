﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoicePortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [HttpPost]
        public async Task<IActionResult> AddAsync(string invoiceId, IFormFileCollection files)
        {
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

                    await _payInvoiceClient.UploadFileAsync(invoiceId, content, formFile.FileName, formFile.ContentType);
                }
            }
            
            return NoContent();
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
