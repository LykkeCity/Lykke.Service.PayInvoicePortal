using System;
using System.Runtime.CompilerServices;
using Common;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.Service.PayInvoicePortal.Core.Extensions
{
    public static class LogExtensions
    {
        public static void ErrorWithDetails(
            this ILog log,
            Exception exception,
            object details,
            [CallerMemberName] string process = null)
        {
            log.Error(exception, exception?.Message, details, process);
        }

        public static void Error(
            this ILog log,
            Exception exception,
            string message,
            object details,
            string process)
        {
            log.Error(process, exception, message, context: $"details: {details.ToJson()}");
        }
    }
}
