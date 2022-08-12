using Common.Application.SagaNotifications;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Common.Application.Events;

namespace ReadModelNotifications.Tests.DefaultCommandNotificationSettingsReader
{
    public class DefaultCommandNotificationSettingsReaderTestsBase
    {
        protected ICommandNotificationSettingsReader _sut;

        protected ICommandNotificationSettingsReader SetupConfigurationSettingsReader(Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            services.AddReadModelNotifications((_) => Mock.Of<IImmediateNotifications>(), (_) => Mock.Of<ISagaNotifications>(), eventOutboxFactory: (_) => Mock.Of<IEventOutbox>());
            configureServices(services);
            return services.BuildServiceProvider().GetRequiredService<ICommandNotificationSettingsReader>();
        }
        protected static string GetNotificationsModeSettingsValue(ReadModelNotificationsMode notificationsMode)
        {
            return notificationsMode switch
            {
                ReadModelNotificationsMode.Saga => "Saga",
                ReadModelNotificationsMode.Immediate => "Immediate",
                ReadModelNotificationsMode.Disabled => "Disabled",
                _ => throw new NotImplementedException()
            };
        }
    }
}
