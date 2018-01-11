using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Pay.Service.Invoces.Client.Models.File;

namespace Lykke.Pay.Invoice.Models
{
    public class FileModel
    {
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string FileExtension { get; set; }
        public string FileUrl { get; set; }

        public FileModel(FileInfoModel filemodel)
        {
            FileName = filemodel.FileName;
            FileSize = (filemodel.FileSize / 1024) + " KB";
            FileExtension = filemodel.FileName.Substring(filemodel.FileName.IndexOf(".") + 1);
            FileUrl = string.Format("/home/invoicefile?invoiceId={0}&fileId={1}&filename={2}", filemodel.InvoiceId, filemodel.FileId, FileName);
        }
    }
}
