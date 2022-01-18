using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using Common.Application.Events;
using Common.Application.Mediator;
using Common.Application.SagaNotifications;
using Core.Query.EventHandlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Application
{
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
            services.AddTransient<IEventOutbox, EventOutbox>();
            services.AddTransient<EventBusHelper>();
            services.AddTransient<EventOutboxSender>();
            services.AddTransient<CommandHandlerBaseDependencies>();
            services.AddTransient<EventConsumerDependencies>();
            services.AddTransient<OptimisticConcurrencyHandler>();
        }

        public static void AddOutboxProcessorService(this IServiceCollection services, EventOutboxProcessorSettings outboxProcessorSettings)
        {
            services.AddSingleton(outboxProcessorSettings);
            services.AddTransient<EventOutboxProcessor>();
            services.AddHostedService<EventOutboxProcessorService>();
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
