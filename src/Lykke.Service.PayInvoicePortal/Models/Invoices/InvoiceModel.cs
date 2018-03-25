﻿using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInvoicePortal.Models.Invoices
{
    public class InvoiceModel
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public double Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string SettlementAsset { get; set; }
        public int SettlementAssetAccuracy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Note { get; set; }
        public List<FileModel> Files { get; set; }
    }
}
