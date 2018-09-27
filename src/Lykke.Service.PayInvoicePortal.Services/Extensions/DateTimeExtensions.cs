using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInvoicePortal.Services.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetBeginOfWeek(this DateTime date)
        {
            var result = date.Date;

            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return result.AddDays(-6);
                case DayOfWeek.Saturday:
                    return result.AddDays(-5);
                case DayOfWeek.Friday:
                    return result.AddDays(-4);
                case DayOfWeek.Thursday:
                    return result.AddDays(-3);
                case DayOfWeek.Wednesday:
                    return result.AddDays(-2);
                case DayOfWeek.Tuesday:
                    return result.AddDays(-1);
                default:
                    return result;
            }
        }

        public static DateTime GetBeginOfMonth(this DateTime date)
        {
            var result = date.Date.AddDays(1 - date.Day);;

            return result;
        }
    }
}
