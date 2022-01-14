using Common.Application;
using Common.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace RabbitMq.EventBus
{
    public static class RabbitMqInstaller
    {
        public static void AddRabbitMq(this IServiceCollection services, RabbitMqSettings rabbitMqSettings)
        {
            services.AddTransient<IAppEventBuilder, AppEventRabbitMQBuilder>();
            services.AddSingleton(rabbitMqSettings);
            services.AddSingleton<RabbitMqEventBus>();
            services.AddSingleton<IEventBus>(s => s.GetRequiredService<RabbitMqEventBus>());
        }

        public static void InitializeEventSubscriptions(IServiceProvider serviceProvider)
        {
            var eventBus = serviceProvider.GetRequiredService<RabbitMqEventBus>();
            eventBus.InitEventSubscriptions(serviceProvider.GetRequiredService<IImplProvider>(), Assembly.GetCallingAssembly());
        }

        public static void InitializeEventSubscriptions(IServiceProvider serviceProvider, params Assembly[] assemblies)
        {
            var eventBus = serviceProvider.GetRequiredService<RabbitMqEventBus>();
            eventBus.InitEventSubscriptions(serviceProvider.GetRequiredService<IImplProvider>(), assemblies);
        }

        public static void InitializeEventConsumers(IServiceProvider serviceProvider)
        {
            var eventBus = serviceProvider.GetRequiredService<RabbitMqEventBus>();
            eventBus.InitEventConsumers(serviceProvider.GetRequiredService<IImplProvider>(), Assembly.GetCallingAssembly());
        }

        public static void InitializeEventConsumers(IServiceProvider serviceProvider, params Assembly[] assemblies)
        {
            var eventBus = serviceProvider.GetRequiredService<RabbitMqEventBus>();
            eventBus.InitEventConsumers(serviceProvider.GetRequiredService<IImplProvider>(), assemblies);
        }
    }
}