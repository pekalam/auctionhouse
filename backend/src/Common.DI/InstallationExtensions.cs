using Adapter.SqlServer.EventOutbox;
using Common.Application.DependencyInjection;
using Common.Application.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.EventBus;
using System.Reflection;
using static System.Convert;

namespace Common.DI
{
    public static class InstallationExtensions
    {
        public static CommonApplicationInstaller AddCommonQueryModule(this IServiceCollection services, IConfiguration configuration, Assembly[] queryHandlerAssemblies, Assembly[] eventConsumerAssemblies)
        {
            var installer = new CommonApplicationInstaller(services);

            installer.AddQueryCoreDependencies(queryHandlerAssemblies)
                .AddRabbitMqAppEventBuilderAdapter()
                .AddRabbitMqEventBusAdapter(configuration, eventConsumerAssemblies: eventConsumerAssemblies);

            return installer;
        }

        public static CommonApplicationInstaller AddCommonCommandModule(this IServiceCollection services, IConfiguration configuration, Assembly[] applicationModuleAssembles)
        {
            var installer = new CommonApplicationInstaller(services);

            installer
                .AddCommandCoreDependencies(applicationModuleAssembles)
                .AddEventOutbox(new EventOutboxProcessorSettings
                {
                    MinMilisecondsDiff = ToInt32(configuration.GetSection(nameof(EventOutboxProcessorSettings))[nameof(EventOutboxProcessorSettings.MinMilisecondsDiff)]),
                    EnableLogging = ToBoolean(configuration.GetSection(nameof(EventOutboxProcessorSettings))[nameof(EventOutboxProcessorSettings.EnableLogging)]),
                })
                .AddSqlServerEventOutboxStorageAdapter(configuration)
                .AddRabbitMqAppEventBuilderAdapter()
                .AddRabbitMqEventBusAdapter(configuration, eventSubscriptionAssemblies: applicationModuleAssembles);

            installer.Services.AddErrorEventRedeliveryProcessorService(new EventBusSettings
            {
                MaxRedelivery = ToInt32(configuration.GetSection(nameof(EventBusSettings))[nameof(EventBusSettings.MaxRedelivery)])
            });

            return installer;
        }
    }
}