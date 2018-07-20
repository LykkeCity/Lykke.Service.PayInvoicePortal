using System;
using Common;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.Service.PayInvoicePortal.Core.Extensions
{
    public static class LogExtensions
    {
        public static void Info(
            this ILog log,
            string message,
            object details)
        {
            log.Info(message, context: $"details: {details.ToJson()}");
        }

        public static void Warning(
            this ILog log,
            string message,
            object details)
        {
            log.Warning(message, null, $"details: {details.ToJson()}");
        }

        public static void Error(
            this ILog log,
            Exception exception,
            object details)
        {
            log.Error(exception, null, $"details: {details.ToJson()}");
        }

        public static void Error(
            this ILog log,
            string message,
            object details)
        {
            log.Error(null, message, $"details: {details.ToJson()}");
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
