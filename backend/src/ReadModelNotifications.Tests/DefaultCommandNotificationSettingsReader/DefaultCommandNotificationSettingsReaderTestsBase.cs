using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Common.Application.Events;
using Microsoft.Extensions.Configuration;
using ReadModelNotifications.Settings;
using ReadModelNotifications.ImmediateNotifications;
using ReadModelNotifications.SagaNotifications;

namespace ReadModelNotifications.Tests.DefaultCommandNotificationSettingsReader
{
    public class DefaultCommandNotificationSettingsReaderTestsBase
    {
        protected ICommandNotificationSettingsReader _sut;

        protected ICommandNotificationSettingsReader SetupConfigurationSettingsReader(IConfigurationRoot configuration)
        {
            var services = new ServiceCollection();
            services.AddCommandReadModelNotifications((_) => Mock.Of<IImmediateNotifications>(), (_) => Mock.Of<ISagaNotifications>(), configuration, eventOutboxFactoryTestDependency: (_) => Mock.Of<IEventOutbox>());
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
