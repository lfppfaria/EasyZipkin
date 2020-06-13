using EasyZipkin.Tracers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using zipkin4net;

namespace EasyZipkin
{
    public class TracerContext
    {
        public static string ServiceName { get; private set; }

        public static Tracer Current { get => ThreadCurrent.Value ?? AsyncLocalTrace.Value; }

        internal static bool HasRemoteTracer { get => _remoteTrace.Any(); }

        private static Stack<Tracer> _tracer { get; set; }

        private static Stack<Trace> _remoteTrace = new Stack<Trace>();

        private static AsyncLocal<Tracer> AsyncLocalTrace = new AsyncLocal<Tracer>();

        private static object _locker;

        internal static ThreadLocal<Tracer> ThreadCurrent = new ThreadLocal<Tracer>();
        
        internal static ThreadLocal<Tracer> ThreadParent = new ThreadLocal<Tracer>();

        public static Tracer GetThreadCurrent()
        {
            return ThreadCurrent.Value;
        }

        public static void SetThreadParent(Tracer current)
        {
            ThreadParent.Value = current;
        }

        internal TracerContext(string serviceName)
        {
            ServiceName = serviceName;

            _tracer = new Stack<Tracer>();

            _locker = new object();
        }

        internal static void Push(Tracer trace)
        {
            lock (_locker)
            {
                _tracer.Push(trace);

                AsyncLocalTrace.Value = trace;
            }
        }

        internal static Trace Pop()
        {
            lock (_locker)
            {
                if (_tracer.Any())
                {
                    var last = _tracer.Pop();

                    if (_tracer.Any())
                    {
                        AsyncLocalTrace.Value = _tracer.Peek();
                    }

                    return last.Trace;
                }

                return null;
            }
        }
  
        public static void RegisterEvent(string name)
        {
            if (ThreadCurrent.Value != null)
            {
                ThreadCurrent.Value.Trace.Record(Annotations.Event(name));

                return;
            }

            var current = _tracer.Peek();

            current.Trace.Record(Annotations.Event(name));
        }
       
        public static void AddTag(string key, string value)
        {
            if (ThreadCurrent.Value != null)
            {
                ThreadCurrent.Value.Trace.Record(Annotations.Tag(key, value));

                return;
            }

            var current = _tracer.Peek();

            current.Trace.Record(Annotations.Tag(key, value));
        }

        internal static void SetRemoteTrace(Trace trace)
        {
            _remoteTrace.Push(trace);
        }

        internal static Trace RetrieveRemoteTrace()
        {
            return _remoteTrace.Pop();
        }
    }
}
