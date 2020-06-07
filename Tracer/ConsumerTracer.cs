using System;
using zipkin4net;

namespace EasyZipkin.Tracer
{
    public class ConsumerTracer : IDisposable
    {
        private readonly Trace _trace;

        public ConsumerTracer(string name)
        {
            _trace = TracerContext.Current.Child();

            TracerContext.Push(_trace);

            _trace.Record(Annotations.Rpc(name));

            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.ConsumerStart());
        }

        public void Dispose()
        {
            _trace.Record(Annotations.ConsumerStop());

            TracerContext.Pop();
        }
    }
}
