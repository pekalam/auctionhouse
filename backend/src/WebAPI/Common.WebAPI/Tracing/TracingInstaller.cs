using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using Microsoft.Extensions.Configuration;

namespace Common.WebAPI.Tracing
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
                    .AddSource(Common.Application.Tracing.GetDefaultActivitySourceName());
            });
        }
    }
}
