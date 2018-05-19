﻿using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PayInvoicePortal.Settings.Clients
{
    public class RateCalculatorServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
