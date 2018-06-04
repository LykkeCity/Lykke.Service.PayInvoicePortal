using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInvoicePortal.Core.Domain.Settings.AppSettings
{
    public class AssetsMapSettings
    {
        public IDictionary<string, string> Values { get; set; }
        public IDictionary<string, string> InvoiceCreationPairs { get; set; }
    }
}
