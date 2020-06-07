using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using EasyZipkin.Helper;

namespace EasyZipkin.Middleware
{
    public class TraceHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.RestoreTracer();

            await _next(httpContext);
        }
    }
}
