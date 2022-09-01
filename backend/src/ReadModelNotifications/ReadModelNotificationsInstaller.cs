using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.ReadModelNotifications;
using Common.Application.SagaNotifications;
using Core.Query.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ReadModelNotifications
{
    public static class ReadModelNotificationsInstaller
    {
        public static void AddReadModelNotifications<TImmediateNotifications, TSagaNotifications>(this IServiceCollection services)
            where TImmediateNotifications : class, IImmediateNotifications
            where TSagaNotifications : class, ISagaNotifications
        {
            services.AddReadModelNotifications(
                static (prov) => prov.GetRequiredService<TImmediateNotifications>(),
                static (prov) => prov.GetRequiredService<TSagaNotifications>());
        }

        /// <summary>
        /// Custom registration of ReadModelNotificaitons
        /// </summary>
        /// <param name="services"></param>
        /// <param name="immediateNotificationsFactory"></param>
        /// <param name="sagaNotificationsFactory"></param>
        /// <param name="commandNotificationSettingsReaderFactory"></param>
        /// <param name="eventOutboxFactoryTestDependency">Test only dependency</param>
        public static void AddReadModelNotifications(
                this IServiceCollection services, 
                Func<IServiceProvider, IImmediateNotifications> immediateNotificationsFactory, 
                Func<IServiceProvider, ISagaNotifications> sagaNotificationsFactory,
                Func<IServiceProvider, ICommandNotificationSettingsReader>? commandNotificationSettingsReaderFactory = null,
                Func<IServiceProvider, IEventOutbox>? eventOutboxFactoryTestDependency = null)
        {
            AddEventOutboxTestDependency(services, eventOutboxFactoryTestDependency);

            AddDefaultCommandNotificationSettingsReader(services, commandNotificationSettingsReaderFactory);

            services.AddScoped<CommandNotificationsEventOutboxWrapper>();
            services.Decorate<IEventOutbox, CommandNotificationsEventOutboxWrapper>();

            services.AddTransient<IEventConsumerCallbacks, ReadModelEventCallbacks>();
            services.AddTransient<ICommandHandlerCallbacks, ReadModelCommandCallbacks>();

            services.AddTransient((prov) => sagaNotificationsFactory(prov));
            services.AddTransient((prov) => immediateNotificationsFactory(prov));
            services.Decorate<ISagaNotifications, TracedSagaNotifications>();
            services.Decorate<IImmediateNotifications, TracedImmediateNotifications>();

            services.Add(new ServiceDescriptor(typeof(ReadModelNotificationsSettings),
                (prov) => new ReadModelNotificationsSettings(prov.GetRequiredService<ICommandNotificationSettingsReader>()), ServiceLifetime.Transient));
        }

        private static void AddEventOutboxTestDependency(IServiceCollection services, Func<IServiceProvider, IEventOutbox>? eventOutboxFactoryTestDependency)
        {
            if (eventOutboxFactoryTestDependency is not null && services.FirstOrDefault(d => d.ServiceType == typeof(IEventOutbox)) is null)
            {
                services.AddTransient(eventOutboxFactoryTestDependency);
            }
        }

        private static void AddDefaultCommandNotificationSettingsReader(IServiceCollection services, Func<IServiceProvider, ICommandNotificationSettingsReader>? commandNotificationSettingsReaderFactory)
        {
            if (commandNotificationSettingsReaderFactory is null)
            {
                services.AddTransient<ConfigurationCommandNotificationSettingsReader>();
                commandNotificationSettingsReaderFactory = static (prov) => prov.GetRequiredService<ConfigurationCommandNotificationSettingsReader>();
            }
            services.AddTransient((prov) => commandNotificationSettingsReaderFactory(prov));
        }
    }
}
