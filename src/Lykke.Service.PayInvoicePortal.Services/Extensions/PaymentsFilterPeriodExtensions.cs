using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.PayInvoicePortal.Core.Domain.Payments;

namespace Lykke.Service.PayInvoicePortal.Services.Extensions
{
    public static class PaymentsFilterPeriodExtensions
    {
        public static (DateTime? DateFrom, DateTime? DateTo) GetDates(this PaymentsFilterPeriod period)
        {
            DateTime? dateFrom = null;
            DateTime? dateTo = null;

            switch (period)
            {
                case PaymentsFilterPeriod.ThisWeek:
                    dateFrom = DateTime.UtcNow.GetBeginOfWeek();
                    break;
                case PaymentsFilterPeriod.LastWeek:
                    dateTo = DateTime.UtcNow.GetBeginOfWeek();
                    dateFrom = dateTo.Value.AddDays(-7);
                    break;
                case PaymentsFilterPeriod.ThisMonth:
                    dateFrom = DateTime.UtcNow.GetBeginOfMonth();
                    break;
                case PaymentsFilterPeriod.LastMonth:
                    dateTo = DateTime.UtcNow.GetBeginOfMonth();
                    dateFrom = dateTo.Value.AddMonths(-1);
                    break;
                case PaymentsFilterPeriod.ThreeMonths:
                    dateFrom = DateTime.UtcNow.Date.AddMonths(-3);
                    break;
                case PaymentsFilterPeriod.ThisYear:
                    dateFrom = new DateTime(DateTime.UtcNow.Year, 1, 1);
                    break;
                case PaymentsFilterPeriod.LastYear:
                    dateTo = new DateTime(DateTime.UtcNow.Year, 1, 1);
                    dateFrom = dateTo.Value.AddYears(-1);
                    break;
            }

            return (dateFrom, dateTo);
        }
    }
}
