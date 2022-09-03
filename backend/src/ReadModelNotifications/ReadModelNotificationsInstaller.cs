using Common.Application.Commands;
using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadModelNotifications.CommandCallbacks;
using ReadModelNotifications.EventCallbacks;
using ReadModelNotifications.EventOutbox;
using ReadModelNotifications.ImmediateNotifications;
using ReadModelNotifications.SagaNotifications;
using ReadModelNotifications.Settings;

namespace ReadModelNotifications
{
    public static class ReadModelNotificationsInstaller
    {
        public static void AddCommandReadModelNotifications<TImmediateNotifications, TSagaNotifications>(this IServiceCollection services, IConfiguration configuration)
            where TImmediateNotifications : class, IImmediateNotifications
            where TSagaNotifications : class, ISagaNotifications
        {
            AddCommandReadModelNotifications(services, static (prov) => prov.GetRequiredService<TImmediateNotifications>(), static (prov) => prov.GetRequiredService<TSagaNotifications>(), configuration);
        }

        public static void AddCommandReadModelNotifications(
            this IServiceCollection services,
            Func<IServiceProvider, IImmediateNotifications> immediateNotificationsFactory,
            Func<IServiceProvider, ISagaNotifications> sagaNotificationsFactory,
            IConfiguration? configuration = null,
            Func<IServiceProvider, ICommandNotificationSettingsReader>? commandNotificationSettingsReaderFactory = null,
            Func<IServiceProvider, IEventOutbox>? eventOutboxFactoryTestDependency = null)
        {
            AddDefaultCommandNotificationSettingsReader(services, configuration, commandNotificationSettingsReaderFactory);
            services.AddScoped<CommandNotificationsEventOutboxWrapper>();
            services.AddTransient<ICommandHandlerCallbacks, ReadModelNotificationsCommandCallbacks>();
            services.Add(new ServiceDescriptor(typeof(ReadModelNotificationsSettings),
                (prov) => new ReadModelNotificationsSettings(prov.GetRequiredService<ICommandNotificationSettingsReader>()), ServiceLifetime.Transient));

            services.AddCommonReadModelNotificationsDependencies(immediateNotificationsFactory, sagaNotificationsFactory, eventOutboxFactoryTestDependency);
            services.Decorate<IEventOutbox, CommandNotificationsEventOutboxWrapper>();
        }


        public static void AddQueryReadModelNotificiations<TImmediateNotifications, TSagaNotifications>(this IServiceCollection services)
            where TImmediateNotifications : class, IImmediateNotifications
            where TSagaNotifications : class, ISagaNotifications
        {
            services.AddQueryReadModelNotificiations(
                static (prov) => prov.GetRequiredService<TImmediateNotifications>(), 
                static (prov) => prov.GetRequiredService<TSagaNotifications>());
        }

        public static void AddQueryReadModelNotificiations(this IServiceCollection services,
                Func<IServiceProvider, IImmediateNotifications> immediateNotificationsFactory,
                Func<IServiceProvider, ISagaNotifications> sagaNotificationsFactory)
        {
            services.AddTransient<IEventConsumerCallbacks, ReadModelNotificationsEventConsumerCallbacks>();
            services.AddCommonReadModelNotificationsDependencies(immediateNotificationsFactory, sagaNotificationsFactory);
        }

        /// <summary>
        /// Custom registration of ReadModelNotificaitons
        /// </summary>
        /// <param name="services"></param>
        /// <param name="immediateNotificationsFactory"></param>
        /// <param name="sagaNotificationsFactory"></param>
        /// <param name="commandNotificationSettingsReaderFactory"></param>
        /// <param name="eventOutboxFactoryTestDependency">Test only dependency</param>
        private static void AddCommonReadModelNotificationsDependencies(
                this IServiceCollection services, 
                Func<IServiceProvider, IImmediateNotifications> immediateNotificationsFactory, 
                Func<IServiceProvider, ISagaNotifications> sagaNotificationsFactory,
                Func<IServiceProvider, IEventOutbox>? eventOutboxFactoryTestDependency = null)
        {
            AddEventOutboxTestDependency(services, eventOutboxFactoryTestDependency);


            services.AddTransient((prov) => sagaNotificationsFactory(prov));
            services.AddTransient((prov) => immediateNotificationsFactory(prov));
            services.Decorate<ISagaNotifications, TracedSagaNotifications>();
            services.Decorate<IImmediateNotifications, TracedImmediateNotifications>();
        }

        private static void AddEventOutboxTestDependency(IServiceCollection services, Func<IServiceProvider, IEventOutbox>? eventOutboxFactoryTestDependency)
        {
            if (eventOutboxFactoryTestDependency is not null && services.FirstOrDefault(d => d.ServiceType == typeof(IEventOutbox)) is null)
            {
                services.AddTransient(eventOutboxFactoryTestDependency);
            }
        }

        private static void AddDefaultCommandNotificationSettingsReader(IServiceCollection services, IConfiguration? configuration, Func<IServiceProvider, ICommandNotificationSettingsReader>? commandNotificationSettingsReaderFactory)
        {
            if (commandNotificationSettingsReaderFactory is null)
            {
                services.AddSingleton(new ConfigurationCommandNotificationSettingsReader(configuration ?? throw new NullReferenceException("Nulll ReadModel notificiations configuration")));
                commandNotificationSettingsReaderFactory = static (prov) => prov.GetRequiredService<ConfigurationCommandNotificationSettingsReader>();
            }
            services.AddTransient((prov) => commandNotificationSettingsReaderFactory(prov));
        }
    }
}
