using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Common.Application.Events;
using ReadModelNotifications.Settings;

namespace ReadModelNotifications.Tests.DefaultCommandNotificationSettingsReader
{
    /// <summary>
    /// Tests DefaultCommandNotificationSettingsReader for Saga configurations
    /// </summary>
    public class DefaultCommandNotificationSettingsReader_SagaTests : DefaultCommandNotificationSettingsReaderTestsBase
    {
        private IConfigurationRoot SetupSagaConfiguration(ReadModelNotificationsMode notificationsMode = ReadModelNotificationsMode.Saga)
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1"},
                {"CommandNotificationSettings:0:NotificationsMode", GetNotificationsModeSettingsValue(notificationsMode)},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:0", "Completion1"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:1", "Completion2"},
                {"CommandNotificationSettings:0:SagaFailureCommandNames:0", "Failure1"},
                {"CommandNotificationSettings:0:SagaFailureCommandNames:1", "Failure2"},
            });

            return config.Build();
        }

        private IConfigurationRoot SetupInvalidSagaConfigurationWithEmptyCompletionNames()
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1"},
                {"CommandNotificationSettings:0:NotificationsMode", "Saga"},
            });

            return config.Build();
        }


        [Fact]
        public void Reads_saga_configuration_from_registered_configuration()
        {
            _sut = SetupConfigurationSettingsReader(SetupSagaConfiguration());

            CommandNotificationSettings[] settings = _sut.Read();

            settings.Length.Should().Be(1);
            settings[0].CommandName.Should().Be("Test1");
            settings[0].NotificationsMode!.Value.Should().Be(ReadModelNotificationsMode.Saga);
            settings[0].SagaCompletionCommandNames.Should().BeEquivalentTo(new[] { "Completion1", "Completion2" });
            settings[0].SagaFailureCommandNames.Should().BeEquivalentTo(new[] { "Failure1", "Failure2" });
        }

        [Fact]
        public void Throws_exception_when_element_in_saga_configuration_does_not_contain_CompletionCommandName()
        {
            _sut = SetupConfigurationSettingsReader(SetupInvalidSagaConfigurationWithEmptyCompletionNames());

            var action = () => _sut.Read();

            action.Should().ThrowExactly<ArgumentException>().And.Message.Should().Be("Null completion command names");
        }
    }
}
