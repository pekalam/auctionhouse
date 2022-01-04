using Common.Application.Events;
using Common.Application.Mediator;
using Common.Application.SagaNotifications;
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
            services.AddTransient<ISagaNotifications, InMemorySagaNotifications>();
            services.AddTransient<IImplProvider, DefaultDIImplProvider>();
            services.AddTransient<EventBusFacade>();
            services.AddMediatR(commandHandlerAssemblies,
                    cfg =>
                    {
                        cfg.AsTransient();
                    });
            services.AddTransient<ImmediateCommandMediator>();
        }

        public static void InitAttributeStrategies(params string[] commandAssemblyNames)
        {
            if(commandAssemblyNames.Length == 0)
            {
                throw new ArgumentException(nameof(commandAssemblyNames));
            }

            CommandMediator.LoadCommandAttributeStrategies(commandAssemblyNames);
        }
    }
}
