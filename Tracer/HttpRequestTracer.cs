using EasyZipkin.Helper;
using System;
using System.Net.Http;
using zipkin4net;

namespace EasyZipkin.Tracer
{
    public class HttpRequestTracer : IDisposable
    {
        private readonly Trace _trace;

        public HttpRequestTracer(HttpRequestMessage request, string name = null)
        {
            _trace = TracerContext.Current.Child();

            request.AddTraceHeaders(_trace);

            _trace.Record(Annotations.Rpc(name));
            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.ClientSend());
        }

        public void Dispose()
        {
            _trace.Record(Annotations.ClientRecv());
        }
    }
}