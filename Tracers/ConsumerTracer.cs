using zipkin4net;

namespace EasyZipkin.Tracers
{
    internal class ConsumerTracer
    {
        private Trace _trace;

        private string _rpc;

        internal void BeginTrace()
        {
            var current = TracerContext.Current;

            _trace = current.Trace.Child();
            _rpc = current.MethodName + "(consuming from queue)";

            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.Rpc(_rpc));
            _trace.Record(Annotations.ConsumerStart());
        }

        internal void EndTrace()
        {
            
            _trace.Record(Annotations.ConsumerStop());
        }
    }
}
