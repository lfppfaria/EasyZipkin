using System;
using System.Net.Http;

namespace EasyZipkin.Tracer
{
    public static class Tracer
    {
        /// <summary>
        /// Use this tracer when you want to trace a remote resource (like a web api) wich is not running Zipzin
        /// and will not be able to resume tracing.
        /// </summary>
        public static T TraceUnmanagedHttpRequest<T>(HttpRequestMessage request, string remoteServiceName, Func<T> function)
        {
            var tracer = new UnmanagedHttpRequestTracer();

            try
            {
                tracer.BeginTrace(request, remoteServiceName);

                return function.Invoke();
            }
            catch (Exception exception)
            {
                TracerContext.AddTag("Exception", exception.Message);
                TracerContext.AddTag("StackTrace", exception.StackTrace);
            }
            finally
            {
                tracer.EndTrace();
            }

            return default;
        }

        /// <summary>
        /// Use this when you will use a remote resource (like a web api) running Zipkin and able to resume tracing.
        /// </summary>
        public static T TraceHttpRequest<T>(HttpRequestMessage request, Func<T> function)
        {
            var tracer = new HttpRequestTracer();

            try
            {
                tracer.BeginTrace(request);

                return function.Invoke();
            }
            catch (Exception exception)
            {
                TracerContext.AddTag("Exception", exception.Message);
                TracerContext.AddTag("StackTrace", exception.StackTrace);
            }
            finally
            {
                tracer.EndTrace();
            }

            return default;
        }

        /// <summary>
        /// Use this to trace procuce time to a MQ provider.
        /// </summary>
        public static void TraceProduce(Action function)
        {
            var tracer = new ProducerTracer();

            try
            {
                tracer.BeginTrace();

                function.Invoke();
            }
            catch (Exception exception)
            {
                TracerContext.AddTag("Exception", exception.Message);
                TracerContext.AddTag("StackTrace", exception.StackTrace);
            }
            finally
            {
                tracer.EndTrace();
            }
        }

        /// <summary>
        /// Use this to trace consume time from a MQ provider.
        /// </summary>
        public static T TraceConsume<T>(Func<T> function)
        {
            var tracer = new ConsumerTracer();

            try
            {
                tracer.BeginTrace();

                return function.Invoke();
            }
            catch (Exception exception)
            {
                TracerContext.AddTag("Exception", exception.Message);
                TracerContext.AddTag("StackTrace", exception.StackTrace);
            }
            finally
            {
                tracer.EndTrace();
            }

            return default;
        }

        /// <summary>
        /// Use this to trace a local resource (like a method or some action).
        /// </summary>
        public static T TraceLocal<T>(string operationName, Func<T> function)
        {
            var tracer = new LocalOperationTracer();

            try
            {
                tracer.BeginTrace(operationName);

                return function.Invoke();
            }
            catch (Exception exception)
            {
                TracerContext.AddTag("Exception", exception.Message);
                TracerContext.AddTag("StackTrace", exception.StackTrace);
            }
            finally
            {
                tracer.EndTrace();
            }

            return default;
        }

        /// <summary>
        /// Use this to trace a local resource (like a method or some action).
        /// </summary>
        public static void TraceLocal(string operationName, Action action)
        {
            var tracer = new LocalOperationTracer();

            try
            {
                tracer.BeginTrace(operationName);

                action.Invoke();
            }
            catch (Exception exception)
            {
                TracerContext.AddTag("Exception", exception.Message);
                TracerContext.AddTag("StackTrace", exception.StackTrace);
            }
            finally
            {
                tracer.EndTrace();
            }
        }
    }
}
