using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using Common.Application.Events;
using Common.Application.Mediator;
using Core.Query.EventHandlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace Common.Application
{
    public class CommonApplicationInstaller
    {
        public CommonApplicationInstaller(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; set; }

        public CommonApplicationInstaller AddEventOutbox(EventOutboxProcessorSettings outboxProcessorSettings)
        {
            Services.AddSingleton(outboxProcessorSettings);
            Services.AddTransient<EventOutboxProcessor>();
            Services.AddHostedService<EventOutboxProcessorService>();

            return this;
        }

        public CommonApplicationInstaller AddCommandCoreDependencies(
            params Assembly[] commandHandlerAssemblies) => 
                AddCommandCoreDependencies(null,null,null,commandHandlerAssemblies);

        public CommonApplicationInstaller AddCommandCoreDependencies(
            Func<IServiceProvider, IEventOutbox>? eventOutboxFactory = null, 
            Func<IServiceProvider, IEventOutboxSavedItems>? eventOutboxSavedItemsFactory = null, 
            Func<IServiceProvider, IImplProvider>? implProviderFactory = null,
            params Assembly[] commandHandlerAssemblies)
        {
            Services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));

            if (implProviderFactory is null) 
            {
                Services.AddTransient<IImplProvider, DefaultDIImplProvider>();
            }
            else
            {
                Services.AddTransient(implProviderFactory);
            }

            Services.AddTransient<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();
            Services.AddTransient<OptimisticConcurrencyHandler>();

            Services.AddMediatR(commandHandlerAssemblies,
            cfg =>
            {
                cfg.AsTransient();
            });

            if(eventOutboxFactory is null)
            {
                Services.AddScoped<EventOutbox>();
                Services.AddScoped<IEventOutbox>(s => s.GetRequiredService<EventOutbox>());
            }
            else
            {
                Services.AddTransient(eventOutboxFactory);
            }

            if(eventOutboxSavedItemsFactory is null)
            {
                Services.AddScoped<IEventOutboxSavedItems>(s => s.GetRequiredService<EventOutbox>());
            }
            else
            {
                Services.AddTransient(eventOutboxSavedItemsFactory);
            }

            Services.AddTransient<EventOutboxSender>();
            Services.AddTransient<ImmediateCommandQueryMediator>();
            Services.AddTransient<CommandHandlerBaseDependencies>();

            Services.AddTransient<ICommandHandlerCallbacks, DefaultCommandHandlerCallbacks>();

            AttributeStrategies.LoadCommandAttributeStrategies(commandHandlerAssemblies);
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers(commandHandlerAssemblies);

            return this;
        }

        public CommonApplicationInstaller AddQueryCoreDependencies(params Assembly[] queryHandlerAssemblies) 
            => AddQueryCoreDependencies(null, queryHandlerAssemblies);

        public CommonApplicationInstaller AddQueryCoreDependencies(
            Func<IServiceProvider, IImplProvider>? implProviderFactory = null,
            params Assembly[] queryHandlerAssemblies)
        {
            Services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            if(implProviderFactory is null)
            {
                Services.AddTransient<IImplProvider, DefaultDIImplProvider>();
            }
            else
            {
                Services.AddTransient(implProviderFactory);
            }
            Services.AddTransient<EventConsumerDependencies>();

            Services.AddMediatR(queryHandlerAssemblies, cfg =>
            {
                cfg.AsTransient();
            });

            Services.AddTransient<ImmediateCommandQueryMediator>();
            AttributeStrategies.LoadQueryAttributeStrategies(queryHandlerAssemblies);
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers(queryHandlerAssemblies);

            return this;
        }

        public CommonApplicationInstaller AddAppEventBuilder(Func<IServiceProvider, IAppEventBuilder> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public CommonApplicationInstaller AddEventBus(Func<IServiceProvider, IEventBus> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public CommonApplicationInstaller AddUserIdentityService(Func<IServiceProvider, IUserIdentityService> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public CommonApplicationInstaller AddOutboxItemFinder(Func<IServiceProvider, IOutboxItemFinder> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public CommonApplicationInstaller AddOutboxItemStore(Func<IServiceProvider, IOutboxItemStore> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }

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

            services.AddMediatR(commandHandlerAssemblies, cfg =>
            {
                cfg.AsTransient();
            });
            services.AddTransient<ImmediateCommandQueryMediator>();
            AttributeStrategies.LoadQueryAttributeStrategies(commandHandlerAssemblies);
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers(commandHandlerAssemblies);
        }

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
