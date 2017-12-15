

using Lykke.Pay.Invoice.AppCode;
using Lykke.Pay.Service.Invoces.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Pay.Invoice.Models
{
    public class InvoiceRequest
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceId { get; set; }

        public string ClientName { get; set; }

        public string ClientEmail { get; set; }

        public double Amount { get; set; }
        public string Currency { get; set; }

        public string Label { get; set; }

        public string DueDate { get; set; }
        public string Status { get; set; }
        public string WalletAddress { get; set; }

        public InvoiceEntity CreateEntity()
        {
            if (string.IsNullOrEmpty(ClientName) || string.IsNullOrEmpty(ClientEmail))
                //string.IsNullOrEmpty(Currency))
            {
                return null;
            }

            return

            new InvoiceEntity
            {
                Amount = Amount,
                ClientEmail = ClientEmail,
                ClientName = ClientName,
                Currency = Currency,
                InvoiceNumber = InvoiceNumber,
                InvoiceId = Guid.NewGuid().ToString(),
                Label = Label,
                Status = Status,
<<<<<<< HEAD
                WalletAddress = WalletAddress,
                DueDate = DateTimeExt.RepoDateStr(DateTime.UtcNow)
=======
                WalletAddress = WalletAddress
>>>>>>> upstream/dev
            };
        }
        public IList<IInvoiceEntity> InvoiceList { get; set; }
    }
}