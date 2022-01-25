using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using Common.Application.Events;
using Common.Application.Mediator;
using Common.Application.ReadModelNotifications;
using Common.Application.SagaNotifications;
using Core.Query.EventHandlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;

namespace Common.Application
{
    public static class Tracing
    {
        internal static ActivitySource? Source { get; private set; }

        internal static void InitializeTracingSource()
        {
            var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name;
            Source = new ActivitySource(assemblyName!);
        }

        public static Activity? StartTracing(string activityName)
        {
            var activity = Source?.StartActivity(ActivityKind.Internal, name: activityName);
            return activity;
        }

        public static Activity? StartTracing(string activityName, CorrelationId correlationId)
        {
            var actCtx = new ActivityContext(ActivityTraceId.CreateFromString(correlationId.Value),
            Activity.Current?.SpanId ?? ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
            var activity = Source?.StartActivity(ActivityKind.Internal, name: activityName, parentContext: actCtx);
            return activity;
        }
    }

    public static class CommonInstaller
    {
        public static void AddCommon(this IServiceCollection services, params Assembly[] commandHandlerAssemblies)
        {
            if(commandHandlerAssemblies.Length == 0)
            {
                throw new ArgumentException($"{nameof(commandHandlerAssemblies)} cannot be an empty array");
            }

            services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            //services.AddTransient<ISagaNotifications, InMemorySagaNotifications>();
            services.AddTransient<IImplProvider, DefaultDIImplProvider>();
            services.AddMediatR(commandHandlerAssemblies,
                    cfg =>
                    {
                        cfg.AsTransient();
                    });
            services.AddTransient<ImmediateCommandQueryMediator>();
            services.AddTransient<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();

            services.AddScoped<EventOutbox>();
            services.AddTransient<IEventOutbox>(s => s.GetRequiredService<EventOutbox>());
            services.AddTransient<IEventOutboxSavedItems>(s => s.GetRequiredService<EventOutbox>());

            services.AddTransient<EventOutboxSender>();
            services.AddTransient<CommandHandlerBaseDependencies>();
            services.AddTransient<EventConsumerDependencies>();
            services.AddTransient<OptimisticConcurrencyHandler>();
        }

        /// <summary>
        /// This method addds decorators used in instrumentation. Should be invoked after adding adapters
        /// </summary>
        public static void AddInstrumentationDecorators(this IServiceCollection services)
        {
            services.Decorate<ISagaNotifications, TracedSagaNotifications>();
            services.Decorate<IImmediateNotifications, TracedImmediateNotifications>();
        }

        public static void AddOutboxProcessorService(this IServiceCollection services, EventOutboxProcessorSettings outboxProcessorSettings)
        {
            services.AddSingleton(outboxProcessorSettings);
            services.AddTransient<EventOutboxProcessor>();
            services.AddHostedService<EventOutboxProcessorService>();
        }


        private static TracerProviderBuilder? _tracerProviderBuilder;
        public static void AddTracing(Action<TracerProviderBuilder>? action = null)
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

        

        public static void InitAttributeStrategies(params string[] commandAssemblyNames)
        {
            if(commandAssemblyNames.Length == 0)
            {
                throw new ArgumentException(nameof(commandAssemblyNames));
            }

            AttributeStrategies.LoadCommandAttributeStrategies(commandAssemblyNames);
            AttributeStrategies.LoadQueryAttributeStrategies(commandAssemblyNames);
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers(commandAssemblyNames);
        }
    }
}
