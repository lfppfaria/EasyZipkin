using EasyZipkin.Helper;
using System.Net.Http;
using zipkin4net;

namespace EasyZipkin.Tracer
{
    internal class HttpRequestTracer
    {
        private Trace _trace;

        internal void BeginTrace(HttpRequestMessage request)
        {
            _trace = TracerContext.Current.Child();

            request.AddTraceHeaders(_trace);

            _trace.Record(Annotations.Rpc(request.RequestUri.AbsoluteUri));
            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.ClientSend());
        }

        internal void EndTrace()
        {
            _trace.Record(Annotations.ClientRecv());
        }
    }
}