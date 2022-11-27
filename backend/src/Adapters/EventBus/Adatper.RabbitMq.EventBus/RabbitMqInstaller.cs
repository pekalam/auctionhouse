using Adatper.RabbitMq.EventBus;
using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using Common.Application;
using Common.Application.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Reflection;

namespace RabbitMq.EventBus
{
    public static class RabbitMqInstaller
    {
        public static CommonApplicationInstaller AddRabbitMqEventBusAdapter(this CommonApplicationInstaller installer,
            IConfiguration? configuration = null,
            RabbitMqSettings? rabbitMqSettings = null,
            Assembly[]? eventSubscriptionAssemblies = null,
            Assembly[]? eventConsumerAssemblies = null)
        {
            rabbitMqSettings ??= configuration!.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();
            rabbitMqSettings.ValidateSettings();

            installer.Services.AddSingleton(rabbitMqSettings);
            installer.Services.AddSingleton<IEasyMQBusInstance, EasyMQBusInstance>();
            installer.Services.AddTransient<RabbitMqEventBus>();
            installer.Services.AddErrorEventOutbox(new());

            installer.AddEventBus(s => s.GetRequiredService<RabbitMqEventBus>());

            if (eventSubscriptionAssemblies != null || eventConsumerAssemblies != null)
            {
                Debug.Assert((eventConsumerAssemblies != null && eventConsumerAssemblies.Length > 0) ||
                    (eventSubscriptionAssemblies != null && eventSubscriptionAssemblies.Length > 0));

                installer.Services.AddHostedService((provider) =>
                {
                    return new RabbitMqSubscriptionInitializer(eventSubscriptionAssemblies, eventConsumerAssemblies, provider);
                });
            }

            return installer;
        }

        public static CommonApplicationInstaller AddRabbitMqAppEventBuilderAdapter(this CommonApplicationInstaller installer)
        {
            installer.Services.AddTransient<AppEventRabbitMQBuilder>();

            installer.AddAppEventBuilder((prov) => prov.GetRequiredService<AppEventRabbitMQBuilder>());

            return installer;
        }

        public static void AddErrorEventRedeliveryProcessorService(this IServiceCollection services, EventBusSettings settings)
        {
            services.AddSingleton(settings);
            services.AddHostedService<ErrorEventOutboxProcessor>();
        }

        internal static void AddRabbitMqEventBus(this IServiceCollection services, RabbitMqSettings rabbitMqSettings)
        {
            services.AddTransient<RabbitMqEventBus>();
            services.AddSingleton(rabbitMqSettings);
            services.AddSingleton<IEasyMQBusInstance, EasyMQBusInstance>();
        }

        internal static void AddErrorEventOutbox(this IServiceCollection services, RocksDbOptions rocksDbOptions)
        {
            RocksDbErrorEventOutboxStorage.Options = rocksDbOptions;
            services.AddSingleton(rocksDbOptions);
            services.AddTransient<IErrorEventOutboxStore, RocksDbErrorEventOutboxStorage>();
            services.AddTransient<IErrorEventOutboxUnprocessedItemsFinder, RocksDbErrorEventOutboxStorage>();
        }

        public static void InitializeEventSubscriptions(IServiceProvider serviceProvider, params Assembly[] assemblies)
        {
            var eventBus = serviceProvider.GetRequiredService<IEasyMQBusInstance>();
            eventBus.InitEventSubscriptions(serviceProvider.GetRequiredService<IImplProvider>(), assemblies);
        }

        public static void InitializeEventConsumers(IServiceProvider serviceProvider, params Assembly[] assemblies)
        {
            var eventBus = serviceProvider.GetRequiredService<IEasyMQBusInstance>();
            eventBus.InitEventConsumers(serviceProvider.GetRequiredService<IImplProvider>(), assemblies);
        }
    }
}