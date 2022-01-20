using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
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
            services.AddRabbitMqEventBus(rabbitMqSettings);
            services.AddErrorEventOutbox(new());
            services.AddSingleton<IEventBus>(s => s.GetRequiredService<RabbitMqEventBus>());
        }

        public static void AddEventRedeliveryProcessorService(this IServiceCollection services)
        {
            services.AddHostedService<ErrorEventOutboxProcessor>();
        }

        internal static void AddRabbitMqEventBus(this IServiceCollection services, RabbitMqSettings rabbitMqSettings)
        {
            services.AddSingleton(rabbitMqSettings);
            services.AddSingleton<RabbitMqEventBus>();
            services.AddTransient<IRabbitMqEventBus>(s => s.GetRequiredService<RabbitMqEventBus>());
        }

        internal static void AddErrorEventOutbox(this IServiceCollection services, RocksDbOptions rocksDbOptions)
        {
            RocksDbErrorEventOutboxStorage.Options = rocksDbOptions;
            services.AddSingleton(rocksDbOptions);
            services.AddTransient<IErrorEventOutboxStore, RocksDbErrorEventOutboxStorage>();
            services.AddTransient<IErrorEventOutboxUnprocessedItemsFinder, RocksDbErrorEventOutboxStorage>();
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