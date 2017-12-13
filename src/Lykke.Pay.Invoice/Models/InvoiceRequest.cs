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

        public string Client { get; set; }

        public string ClientEmail { get; set; }

        public double Amount { get; set; }
        public string Currency { get; set; }

        public string Label { get; set; }

        public string DueDate { get; set; }

        public InvoiceEntity CreateEntity()
        {
            if (string.IsNullOrEmpty(Client) || string.IsNullOrEmpty(ClientEmail))
                //string.IsNullOrEmpty(Currency))
            {
                return null;
            }

            return

            new InvoiceEntity
            {
                Amount = Amount,
                ClientEmail = ClientEmail,
                ClientName = Client,
                Currency = Currency,
                InvoiceNumber = InvoiceNumber,
                InvoiceId = Guid.NewGuid().ToString(),
                DueDate = DueDate,
                Label = Label
            };
        }
    }
}