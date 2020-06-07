using zipkin4net;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace EasyZipkin.Builder
{
    //public static class Interceptor
    //{
    //    public static void RegisterInterceptor(this IServiceCollection services)
    //    {
    //        services.EnableSimpleProxy(p => p.AddInterceptor<TraceAttribute, TraceInterceptor>());
    //    }
    //}


    public class TracerContextBuilder
    {
        private string _collectorUri;

        private string _serviceName;

        public static string ServiceName { get; private set; }

        /// <summary>
        /// Cria um novo builder.
        /// </summary>
        public TracerContextBuilder()
        {

        }

        /// <summary>
        /// Informa o endereço onde o coletor do Zipkin está rodando.
        /// </summary>
        /// <param name="collectorUri">Endereço onde o coletor do Zipkin está rodando.</param>
        /// <returns>Instância do ZipkinContextBuilder em uso.</returns>
        public TracerContextBuilder WithCollectorUri(string collectorUri)
        {
            _collectorUri = collectorUri;

            return this;
        }

        /// <summary>
        /// Informa o nome do serviço que será registrado no Zipkin.
        /// </summary>
        /// <param name="serviceName">Nome do serviço.</param>
        /// <returns>Instância do ZipkinContextBuilder em uso.</returns>
        public TracerContextBuilder WithServiceName(string serviceName)
        {
            _serviceName = serviceName;

            return this;
        }

        /// <summary>
        /// Cria o contexto com base nos dados informados.
        /// </summary>
        public void Build()
        {
            ServiceName = _serviceName;

            var sender = new HttpZipkinSender(_collectorUri, "application/json");

            TraceManager.SamplingRate = 1.0f;

            var spanSerializer = new JSONSpanSerializer();

            var tracer = new ZipkinTracer(sender, spanSerializer);

            var zipkinLogger = new ZipkinLogger();

            TraceManager.RegisterTracer(tracer);

            TraceManager.Start(zipkinLogger);

            new TracerContext(_serviceName);
        }
    }
}
