﻿using System;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class InvoiceListItemModel
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public double Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}