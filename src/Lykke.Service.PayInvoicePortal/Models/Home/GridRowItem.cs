using System;

namespace Lykke.Service.PayInvoicePortal.Models.Home
{
    public class GridRowItem
    {
        public string Id { get; set; }

        public string Number { get; set; }

        public string ClientName { get; set; }

        public string ClientEmail { get; set; }

        public Decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; }

        public string SettlementAssetId { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
