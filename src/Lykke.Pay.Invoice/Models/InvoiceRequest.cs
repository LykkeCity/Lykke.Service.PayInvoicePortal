

using Lykke.Pay.Invoice.AppCode;
using Lykke.Pay.Service.Invoces.Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public string Amount { get; set; }
        public string Currency { get; set; }

        public string Label { get; set; }

        public string DueDate { get; set; }
        public string Status { get; set; }
        public string WalletAddress { get; set; }
        public string StartDate { get; set; }

        public InvoiceEntity CreateEntity()
        {
            if (string.IsNullOrEmpty(ClientName) || string.IsNullOrEmpty(ClientEmail))
                //string.IsNullOrEmpty(Currency))
            {
                return null;
            }
            var invoiceid = string.IsNullOrEmpty(InvoiceId) ? Guid.NewGuid().ToString() : InvoiceId;
            return new InvoiceEntity
            {
                Amount = double.Parse(Amount, CultureInfo.InvariantCulture),
                ClientEmail = ClientEmail,
                ClientName = ClientName,
                Currency = Currency,
                InvoiceNumber = InvoiceNumber,
                InvoiceId = invoiceid,
                Label = Label,
                Status = Status,
                WalletAddress = WalletAddress,
                StartDate = DateTime.Today.RepoDateStr()
            };
        }
    }
}