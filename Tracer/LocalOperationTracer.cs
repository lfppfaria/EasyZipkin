using System;
using zipkin4net;

namespace EasyZipkin.Tracer
{
    public class LocalOperationTracer : IDisposable
    {
        private readonly Trace _trace;

        public LocalOperationTracer(string operationName)
        {
            _trace = Trace.Current.Child();

            _trace.Record(Annotations.LocalOperationStart(operationName));
        }

        public void Dispose()
        {
            _trace.Record(Annotations.LocalOperationStop());
        }
    }
}
