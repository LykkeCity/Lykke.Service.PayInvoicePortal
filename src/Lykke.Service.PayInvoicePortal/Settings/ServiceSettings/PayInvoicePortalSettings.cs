﻿using System;
using Lykke.Service.PayInvoicePortal.Settings.ServiceSettings.Db;

namespace Lykke.Service.PayInvoicePortal.Settings.ServiceSettings
{
    public class PayInvoicePortalSettings
    {
        public string SiteUrl { get; set; }

        public TimeSpan UserLoginTime { get; set; }

        public TimeSpan OrderLiveTime { get; set; }
        
        public DbSettings Db { get; set; }
    }
}
