using System;
using System.Globalization;
using System.Net.Http;
using zipkin4net;

namespace EasyZipkin.Tracers
{
    internal class UnmanagedHttpRequestTracer
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        private Trace _trace;

        private string _entryDate;
        private string _remoteServiceName;
        private string _rpc;

        internal void BeginTrace(HttpRequestMessage request, string remoteServiceName)
        {
            _entryDate = DateTime.Now.ToString(DateFormat, CultureInfo.InvariantCulture);

            _remoteServiceName = remoteServiceName;

            _rpc = request.RequestUri.AbsoluteUri;

            _trace = TracerContext.Current.Trace.Child();

            _trace.Record(Annotations.ServiceName(_remoteServiceName));
            _trace.Record(Annotations.Rpc(_rpc));
            _trace.Record(Annotations.ServerRecv());
        }

        internal void EndTrace()
        {
            _trace.Record(Annotations.ServerSend());

            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.Rpc(_rpc));
            _trace.Record(Annotations.ClientSend(), DateTime.ParseExact(_entryDate, DateFormat, CultureInfo.InvariantCulture));

            _trace.Record(Annotations.ClientRecv(), DateTime.Now);
        }
    }
}