using System;
using System.Threading.Tasks;
using Common.Log;
using Microsoft.AspNetCore.Http;

namespace Lykke.Pay.Invoice
{
    public class ApiTraceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _log;

        public ApiTraceMiddleware(RequestDelegate next, ILog log)
        {
            _next = next;
            _log = log;
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
                await _log.WriteErrorAsync(request.Host.ToString(), request.Path, ex);
                throw;
            }
        }
    }
}