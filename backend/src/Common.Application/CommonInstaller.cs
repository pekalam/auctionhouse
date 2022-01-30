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
using System.Reflection;

namespace Common.Application
{

    public static class CommonInstaller
    {
        /// <summary>
        /// Registers common dependencies for query application
        /// </summary>
        public static void AddCommonQueryDependencies(this IServiceCollection services, params Assembly[] commandHandlerAssemblies)
        {
            if (commandHandlerAssemblies.Length == 0)
            {
                throw new ArgumentException($"{nameof(commandHandlerAssemblies)} cannot be an empty array");
            }

            services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            services.AddTransient<IImplProvider, DefaultDIImplProvider>();
            services.AddTransient<EventConsumerDependencies>();

            services.AddMediatR(commandHandlerAssemblies,
cfg =>
            {
                cfg.AsTransient();
            });
            services.AddTransient<ImmediateCommandQueryMediator>();
            AttributeStrategies.LoadQueryAttributeStrategies(commandHandlerAssemblies);
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers(commandHandlerAssemblies);
        }

        /// <summary>
        /// Registers common dependencies for command application
        /// </summary>
        public static void AddCommonCommandDependencies(this IServiceCollection services, params Assembly[] commandHandlerAssemblies)
        {
            if(commandHandlerAssemblies.Length == 0)
            {
                throw new ArgumentException($"{nameof(commandHandlerAssemblies)} cannot be an empty array");
            }

            services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            services.AddTransient<IImplProvider, DefaultDIImplProvider>();


            services.AddConcurrencyUtils();
            services.AddCommandHandling<EventOutbox>(commandHandlerAssemblies);
            AttributeStrategies.LoadCommandAttributeStrategies(commandHandlerAssemblies);
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers(commandHandlerAssemblies);
        }

        public static void AddConcurrencyUtils(this IServiceCollection services)
        {
            services.AddTransient<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();
            services.AddTransient<OptimisticConcurrencyHandler>();
        }

        public static void AddCommandHandling<TEventOutbox>(this IServiceCollection services, params Assembly[] commandHandlerAssemblies)
            where TEventOutbox : class, IEventOutbox, IEventOutboxSavedItems
        {
            services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            services.AddMediatR(commandHandlerAssemblies,
        cfg =>
        {
            cfg.AsTransient();
        });
            services.AddScoped<TEventOutbox>();
            services.AddTransient<IEventOutbox>(s => s.GetRequiredService<TEventOutbox>());
            services.AddTransient<IEventOutboxSavedItems>(s => s.GetRequiredService<TEventOutbox>());
            services.AddTransient<EventOutboxSender>();
            services.AddTransient<ImmediateCommandQueryMediator>();
            services.AddTransient<CommandHandlerBaseDependencies>();
        }

        public static void AddOutboxProcessorService(this IServiceCollection services, EventOutboxProcessorSettings outboxProcessorSettings)
        {
            services.AddSingleton(outboxProcessorSettings);
            services.AddTransient<EventOutboxProcessor>();
            services.AddHostedService<EventOutboxProcessorService>();
        }


        private static TracerProviderBuilder? _tracerProviderBuilder;

        public static void AddTracing(this IServiceCollection services, Action<TracerProviderBuilder>? action = null)
        {
            Tracing.InitializeTracingSource();
            _tracerProviderBuilder = Sdk.CreateTracerProviderBuilder();
            services.Decorate<ISagaNotifications, TracedSagaNotifications>();
            services.Decorate<IImmediateNotifications, TracedImmediateNotifications>();
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
