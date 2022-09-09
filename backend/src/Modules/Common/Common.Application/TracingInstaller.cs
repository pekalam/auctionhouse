using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Application
{
    public static class TracingInstaller
    {
        private static TracerProviderBuilder? _tracerProviderBuilder;

        public static void AddTracing(this IServiceCollection services, Action<TracerProviderBuilder>? action = null)
        {
            Tracing.InitializeTracingSource();
            _tracerProviderBuilder = Sdk.CreateTracerProviderBuilder();
            action?.Invoke(_tracerProviderBuilder);
        }

        public static TracerProvider CreateModuleTracing(string serviceName)
        {
            return (_tracerProviderBuilder ?? Sdk.CreateTracerProviderBuilder())
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddZipkinExporter()
                .AddSource(Tracing.Source.Name)
                .Build();
        }
    }
}
