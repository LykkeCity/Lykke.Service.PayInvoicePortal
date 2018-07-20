using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.PayInvoicePortal
{
    public class ApiTraceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _log;

        public ApiTraceMiddleware(RequestDelegate next, ILogFactory logFactory)
        {
            _next = next;
            _log = logFactory.CreateLog(this);
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }
    }
}
