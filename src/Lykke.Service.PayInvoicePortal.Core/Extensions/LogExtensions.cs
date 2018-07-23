using System;
using Common;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.Service.PayInvoicePortal.Core.Extensions
{
    public static class LogExtensions
    {
        public static void Error(
            this ILog log,
            Exception exception,
            object details)
        {
            log.Error(exception, null, details);
        }

        public static void Error(
            this ILog log,
            Exception exception,
            string message,
            object details)
        {
            log.Error(exception, message, context: $"details: {details.ToJson()}");
        }
    }
}
