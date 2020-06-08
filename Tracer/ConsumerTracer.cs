using zipkin4net;

namespace EasyZipkin.Tracer
{
    internal class ConsumerTracer
    {
        private Trace _trace;

        internal void BeginTrace()
        {
            _trace = TracerContext.Current;

            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.ConsumerStart());
        }

        internal void EndTrace()
        {
            _trace.Record(Annotations.ConsumerStop());
        }
    }
}
