using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInvoice.Client.Models.File;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Domain.Incoming;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> GetByIdAsync(string invoiceId);

        Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(string merchantId, string invoiceId);

        Task<IReadOnlyList<FileInfoModel>> GetFilesAsync(string invoiceId);

        Task<PaymentDetails> GetPaymentDetailsAsync(string invoiceId, bool force);

        Task<InvoiceStatusModel> GetStatusAsync(string invoiceId);

        Task<InvoiceModel> ChangePaymentAssetAsync(string invoiceId, string paymentRequestId);

        Task<Invoice> CreateAsync(CreateInvoiceModel model, bool draft);

        Task UpdateAsync(UpdateInvoiceModel model, bool draft);

        Task UploadFileAsync(string invoiceId, byte[] content, string fileName, string contentType);

        Task DeleteAsync(string invoiceId);

        Task<IncomingInvoicesSource> GetIncomingAsync(
            string merchantId,
            IReadOnlyList<InvoiceStatus> statuses,
            Period period,
            string searchValue,
            int skip,
            int take);

        Task<IReadOnlyList<Invoice>> GetAsync(
            string merchantId,
            IReadOnlyList<InvoiceStatus> status,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending);

        Task<InvoiceSource> GetSupervisingAsync(
            string merchantId,
            string employeeId,
            IReadOnlyList<InvoiceStatus> status,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending,
            int skip,
            int take);
    }
}
