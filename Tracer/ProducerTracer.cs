using System;
using zipkin4net;

namespace EasyZipkin.Tracer
{
    public class ProducerTracer : IDisposable
    {
        private readonly Trace _trace;

        public ProducerTracer(string name)
        {
            _trace = TracerContext.Current.Child();

            TracerContext.Push(_trace);

            _trace.Record(Annotations.Rpc(name));
            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));            
            _trace.Record(Annotations.ProducerStart());
        }

        public void Dispose()
        {
            _trace.Record(Annotations.ProducerStop());

            TracerContext.Pop();
        }
    }
}
