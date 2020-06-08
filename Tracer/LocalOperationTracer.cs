using System;
using zipkin4net;

namespace EasyZipkin.Tracer
{
    internal class LocalOperationTracer
    {
        private Trace _trace;

        internal void BeginTrace(string operationName)
        {
            _trace = Trace.Current.Child();

            _trace.Record(Annotations.LocalOperationStart(operationName));
        }

        internal void EndTrace()
        {
            _trace.Record(Annotations.LocalOperationStop());
        }
    }
}
