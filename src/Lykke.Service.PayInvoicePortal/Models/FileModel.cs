using System.IO;
using Lykke.Service.PayInvoice.Client.Models.File;

namespace Lykke.Service.PayInvoicePortal.Models
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
            FileSize = $"{filemodel.FileSize / 1024} KB";
            FileExtension = Path.GetFileNameWithoutExtension(filemodel.FileName);
            FileUrl = $"/home/invoicefile?invoiceId={filemodel.InvoiceId}&fileId={filemodel.FileId}&filename={FileName}";
        }
    }
}
