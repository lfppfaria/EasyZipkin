using EasyZipkin.Tracer;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using zipkin4net;

namespace EasyZipkin
{
    public class TracerContext
    {
        public static string ServiceName { get; private set; }

        internal static Trace Current { get => AsyncLocalTrace.Value; }

        internal static bool HasRemoteTracer { get => _remoteTrace.Any(); }

        private static Stack<Trace> _traces { get; set; }

        private static Stack<Trace> _remoteTrace = new Stack<Trace>();

        private static AsyncLocal<Trace> AsyncLocalTrace = new AsyncLocal<Trace>();

        internal TracerContext(string serviceName)
        {
            ServiceName = serviceName;

            _traces = new Stack<Trace>();
        }

        internal static void Push(Trace trace)
        {
            _traces.Push(trace);

            AsyncLocalTrace.Value = trace;
        }

        internal static Trace Pop()
        {
            if (_traces.Any())
            {
                var last = _traces.Pop();

                if (_traces.Any())
                    AsyncLocalTrace.Value = _traces.Peek();

                return last;
            }

            return null;
        }

        /// <summary>
        /// Escreve um evento como uma ocorrência do contexto atual.
        /// </summary>
        /// <param name="name">Nome do evento.</param>
        public static void RegisterEvent(string name)
        {
            var current = _traces.Peek();

            current.Record(Annotations.Event(name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddTag(string key, string value)
        {
            var current = _traces.Peek();

            current.Record(Annotations.Tag(key, value));
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
