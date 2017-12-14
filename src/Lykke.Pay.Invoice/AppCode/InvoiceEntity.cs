using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Pay.Service.Invoces.Client.Models;

namespace Lykke.Pay.Invoice.AppCode
{
    public static class InvoiceEntityExt
    {
        public static InvoiceEntity CreateInvoiceEntity(this IInvoiceEntity inv)
        {
            return new InvoiceEntity()
            {
                InvoiceId = inv.InvoiceId,
                InvoiceNumber = inv.InvoiceNumber,
                Amount = inv.Amount,
                Currency = inv.Currency,
                ClientId = inv.ClientId,
                ClientName = inv.ClientName,
                ClientUserId = inv.ClientUserId,
                ClientEmail = inv.ClientEmail,
                DueDate = inv.DueDate,
                Label = inv.Label,
                Status = inv.Status,
                WalletAddress = inv.WalletAddress,
                StartDate = inv.StartDate
        };
        }
    }
}
