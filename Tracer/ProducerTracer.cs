using System;
using zipkin4net;

namespace EasyZipkin.Tracer
{
    public class ProducerTracer : IDisposable
    {
        private readonly Trace _trace;

        public ProducerTracer()
        {
            _trace = TracerContext.Current;

            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.ProducerStart());
        }

        public void Dispose()
        {
            _trace.Record(Annotations.ProducerStop());
        }
    }
}
