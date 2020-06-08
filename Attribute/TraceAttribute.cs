using MethodBoundaryAspect.Fody.Attributes;
using zipkin4net;

namespace EasyZipkin.Attribute
{
    public class TraceAttribute : OnMethodBoundaryAspect
    {
        private string _operationName;

        private Trace _trace;

        private bool _remote;

        public TraceAttribute(string operationName = null)
        {
            _operationName = operationName;
        }

        public override void OnEntry(MethodExecutionArgs arg)
        {
            if (string.IsNullOrEmpty(_operationName))
                _operationName = arg.Method.Name;

            _remote = TracerContext.HasRemoteTracer;

            if (_remote)
                _trace = TracerContext.RetrieveRemoteTrace();

            if (_trace == null)
                _trace = TracerContext.Current?.Child() ?? Trace.Create();

            TracerContext.Push(_trace);

            _trace.Record(Annotations.ServiceName(TracerContext.ServiceName));
            _trace.Record(Annotations.Rpc(_operationName));

            if (_remote)
                _trace.Record(Annotations.ServerRecv());
            else
                _trace.Record(Annotations.LocalOperationStart(_operationName));
        }

        public override void OnExit(MethodExecutionArgs arg)
        {
            if (_remote)
                _trace.Record(Annotations.ServerSend());
            else
                _trace.Record(Annotations.LocalOperationStop());

            TracerContext.Pop();
        }

        public override void OnException(MethodExecutionArgs arg)
        {
            var exception = arg.Exception;

            _trace.Record(Annotations.Tag("Exception", exception.Message));
            _trace.Record(Annotations.Tag("StackTrace", exception.StackTrace));
        }
    }
}