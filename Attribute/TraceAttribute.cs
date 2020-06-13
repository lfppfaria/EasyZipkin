using EasyZipkin.Tracers;
using MethodBoundaryAspect.Fody.Attributes;
using zipkin4net;

namespace EasyZipkin.Attribute
{
    public class TraceAttribute : OnMethodBoundaryAspect
    {
        private string _operationName;

        private bool _setCurrent;

        private Trace _trace;

        private bool _remote;

        public TraceAttribute(string operationName = null, bool setCurrent = true)
        {
            _operationName = operationName;

            _setCurrent = setCurrent;
        }

        public override void OnEntry(MethodExecutionArgs arg)
        {
            if (string.IsNullOrEmpty(_operationName))
                _operationName = arg.Method.Name;

            _remote = TracerContext.HasRemoteTracer;

            if (_remote)
                _trace = TracerContext.RetrieveRemoteTrace();

            if (TracerContext.ThreadParent.Value != null)
                _trace = TracerContext.ThreadParent.Value.Trace.Child();

            if (_trace == null)
                _trace = TracerContext.Current?.Trace.Child() ?? Trace.Create();

            if (_setCurrent)
                TracerContext.Push(new Tracer { MethodName = _operationName, Trace = _trace });

            TracerContext.ThreadCurrent.Value = new Tracer { MethodName = _operationName, Trace = _trace };

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

            if (_setCurrent)
                TracerContext.Pop();

            TracerContext.ThreadCurrent.Value = null;
            TracerContext.ThreadParent.Value = null;
        }

        public override void OnException(MethodExecutionArgs arg)
        {
            var exception = arg.Exception;

            _trace.Record(Annotations.Tag("Exception", exception.Message));
            _trace.Record(Annotations.Tag("StackTrace", exception.StackTrace));
        }
    }
}