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

        public static void AddReadModelNotifications(
                this IServiceCollection services, 
                Func<IServiceProvider, IImmediateNotifications> immediateNotificationsFactory, 
                Func<IServiceProvider, ISagaNotifications> sagaNotificationsFactory,
                Func<IServiceProvider, ICommandNotificationSettingsReader>? commandNotificationSettingsReaderFactory = null,
                Func<IServiceProvider, IEventOutbox>? eventOutboxFactory = null)
        {
            // Module required dependency for tests
            if (eventOutboxFactory is not null && services.FirstOrDefault(d => d.ServiceType == typeof(IEventOutbox)) is null)
            {
                services.AddTransient(eventOutboxFactory);
            }

            if (commandNotificationSettingsReaderFactory is null)
            {
                services.AddTransient<ConfigurationCommandNotificationSettingsReader>();
                commandNotificationSettingsReaderFactory = static (prov) => prov.GetRequiredService<ConfigurationCommandNotificationSettingsReader>();
            }

            services.AddScoped<CommandNotificationsEventOutboxWrapper>();
            services.Decorate<IEventOutbox, CommandNotificationsEventOutboxWrapper>();

            services.AddTransient<ICommandNotificationSettingsReader>((prov) => commandNotificationSettingsReaderFactory(prov));
            services.AddTransient<IEventConsumerCallbacks, ReadModelEventCallbacks>();
            services.AddTransient<ICommandHandlerCallbacks, ReadModelCommandCallbacks>();

            services.AddTransient<ISagaNotifications>((prov) => sagaNotificationsFactory(prov));
            services.AddTransient<IImmediateNotifications>((prov) => immediateNotificationsFactory(prov));
            services.Decorate<ISagaNotifications, TracedSagaNotifications>();
            services.Decorate<IImmediateNotifications, TracedImmediateNotifications>();

            services.Add(new ServiceDescriptor(typeof(ReadModelNotificationsSettings), 
                (prov) => new ReadModelNotificationsSettings(prov.GetRequiredService<ICommandNotificationSettingsReader>()), ServiceLifetime.Transient));
        }
    }
}
