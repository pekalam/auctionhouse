using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Configuration;

namespace WebAPI.Common.Tracing
{
    public static class TracingInstaller
    {

        public static void AddTracing(this IServiceCollection services, string serviceName, IConfiguration configuration)
        {
            services.AddOpenTelemetryTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddOtlpExporter(opt =>
                    {
                        configuration.GetSection("OpenTelemetry:OtlpExporter").Bind(opt);
                    })
                    .AddAspNetCoreInstrumentation()
                    .AddSource(global::Common.Application.Tracing.GetDefaultActivitySourceName());
            });
        }
    }
}
