using Common.Application.Events;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace ReadModelNotifications.Tests.DefaultCommandNotificationSettingsReader
{
    public class DefaultCommandNotificationSettingsReader_Tests : DefaultCommandNotificationSettingsReaderTestsBase
    {
        private IConfigurationRoot SetupInvalidConfigurationWithMissingNotificationsMode()
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:0", "Completion1"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:1", "Completion2"},
            });

            return config.Build();
        }

        private IConfigurationRoot SetupInvalidConfigurationWithInvalidNotificationsMode()
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1"},
                {"CommandNotificationSettings:0:NotificationsMode", "invalid"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:0", "Completion1"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:1", "Completion2"},
            });

            return config.Build();
        }

        private IConfigurationRoot SetupConfiguration(ReadModelNotificationsMode notificationsMode)
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1"},
                {"CommandNotificationSettings:0:NotificationsMode", GetNotificationsModeSettingsValue(notificationsMode)},
            });

            return config.Build();
        }

        private IConfigurationRoot SetupConfigurationWithCompletionCommandNames(ReadModelNotificationsMode notificationsMode)
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:0", "Completion1"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:1", "Completion2"},
                {"CommandNotificationSettings:0:NotificationsMode", GetNotificationsModeSettingsValue(notificationsMode)},
            });

            return config.Build();
        }

        private IConfigurationRoot SetupEmptyConfifguration()
        {
            return new ConfigurationBuilder().Build();
        }

        [Theory]
        [InlineData(ReadModelNotificationsMode.Disabled)]
        [InlineData(ReadModelNotificationsMode.Immediate)]
        public void Reads_configuration_from_registered_configuration_with_mode(ReadModelNotificationsMode notificationsMode)
        {
            _sut = SetupConfigurationSettingsReader(SetupConfiguration(notificationsMode));

            CommandNotificationSettings[] settings = _sut.Read();

            settings.Length.Should().Be(1);
            settings[0].CommandName.Should().Be("Test1");
            settings[0].NotificationsMode!.Value.Should().Be(notificationsMode);
            settings[0].SagaCompletionCommandNames.Should().BeNull();
            settings[0].SagaFailureCommandNames.Should().BeNull();
        }

        [Theory]
        [InlineData(ReadModelNotificationsMode.Immediate)]
        [InlineData(ReadModelNotificationsMode.Disabled)]
        public void Throws_exception_when_element_in_configuration_contains_CompletionCommandName_and_has_mode(ReadModelNotificationsMode notificationsMode)
        {
            _sut = SetupConfigurationSettingsReader(SetupConfigurationWithCompletionCommandNames(notificationsMode));

            var action = () => _sut.Read();

            action.Should().ThrowExactly<ArgumentException>().And.Message.Should().Be("Cannot contain other notifications mode than saga and have completion or failure command names");
        }

        [Fact]
        public void Throws_exception_when_element_in_configuration_does_not_contain_NotificaitonsMode()
        {
            _sut = SetupConfigurationSettingsReader(SetupInvalidConfigurationWithMissingNotificationsMode());

            var action = () => _sut.Read();

            action.Should().ThrowExactly<ArgumentException>().And.Message.Should().Be("Missing NotificationsMode");
        }

        [Fact]
        public void Throws_exception_when_element_in_configuration_contains_invalid_NotifcationsMode()
        {
            _sut = SetupConfigurationSettingsReader(SetupInvalidConfigurationWithInvalidNotificationsMode());

            var action = () => _sut.Read();

            action.Should().ThrowExactly<ArgumentException>().And.Message.Should().Be("Invalid configuration");
        }

        [Fact]
        public void Return_empty_configuration_if_registered_configuration_is_empty()
        {
            _sut = SetupConfigurationSettingsReader(SetupEmptyConfifguration());

            var settings = _sut.Read();

            settings.Should().BeEmpty();
        }
    }
}
