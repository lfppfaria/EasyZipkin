using Microsoft.AspNetCore.Http;
using System.Net.Http;
using zipkin4net;

namespace EasyZipkin.Helper
{
    public static class HttpHelper
    {
        internal static void AddTraceHeaders(this HttpRequestMessage httpRequestMessage, Trace trace)
        {
            var traceId = trace.CurrentSpan.TraceId;

            var parentSpanId = trace.CurrentSpan.ParentSpanId;

            var spanId = trace.CurrentSpan.SpanId;

            var sampled = trace.CurrentSpan.Sampled;

            var debug = trace.CurrentSpan.Debug;

            var httpRequestHeaders = httpRequestMessage.Headers;

            httpRequestHeaders.Add("zipkin_traceId", traceId.ToString());
            httpRequestHeaders.Add("zipkin_parentSpanId", parentSpanId.ToString());
            httpRequestHeaders.Add("zipkin_spanId", spanId.ToString());
            httpRequestHeaders.Add("zipkin_sampled", sampled.ToString());
            httpRequestHeaders.Add("zipkin_debug", debug.ToString());
        }

        internal static void RestoreTracer(this HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.ContainsKey("zipkin_traceId") && !httpContext.Request.Headers.ContainsKey("zipkin_spanId"))
                return;

            var traceId = httpContext.Request.Headers["zipkin_traceId"];

            var parentSpanId = httpContext.Request.Headers["zipkin_parentSpanId"];

            var spanId = httpContext.Request.Headers["zipkin_spanId"];

            var sampled = httpContext.Request.Headers["zipkin_sampled"];

            var debug = httpContext.Request.Headers["zipkin_debug"];

            var spanState = new SpanState(
                traceId: long.Parse(traceId),
                parentSpanId: !string.IsNullOrWhiteSpace(parentSpanId.ToString()) ? long.Parse(parentSpanId) : default,
                spanId: long.Parse(spanId),
                isSampled: !string.IsNullOrWhiteSpace(sampled.ToString()) ? bool.Parse(sampled) : default,
                isDebug: bool.Parse(debug));

            var restored = Trace.CreateFromId(spanState);

            TracerContext.SetRemoteTrace(restored);
        }
    }
}
