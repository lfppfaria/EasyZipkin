using System;
using System.Net.Http;
using zipkin4net;
using EasyZipkin;
using EasyZipkin.Helper;

namespace EasyZipkin.Tracer
{
    public class HttpRequestTracer : IDisposable
    {
        private readonly Trace _trace;

        public HttpRequestTracer(HttpRequestMessage request)
        {
            _trace = TracerContext.Current.Child();

            request.AddTraceHeaders(_trace);

            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.ClientSend());
        }

        public void Dispose()
        {
            _trace.Record(Annotations.ClientRecv());
        }
    }
}