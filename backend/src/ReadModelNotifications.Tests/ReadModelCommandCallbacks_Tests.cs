using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ReadModelNotifications.Tests
{
    public class ReadModelCommandCallbacks_Tests
    {
        private class StubNotificationReader : ICommandNotificationSettingsReader
        {
            private readonly CommandNotificationSettings[] _notificationSettings;

            public StubNotificationReader(CommandNotificationSettings[] notificationSettings)
            {
                _notificationSettings = notificationSettings;
            }

            public CommandNotificationSettings[] Read()
            {
                return _notificationSettings;
            }
        }

        private class TestCommand1 : ICommand
        {

        }

        private class TestCommand2 : ICommand
        {

        }

        private const string SagaInitiatorKey = "SagaInitiator";
        private readonly Mock<IImmediateNotifications> _mockImmediateNotifications;
        private readonly Mock<ISagaNotifications> _mockSagaNotifications;

        public ReadModelCommandCallbacks_Tests()
        {
            _mockImmediateNotifications = new Mock<IImmediateNotifications>();
            _mockSagaNotifications = new Mock<ISagaNotifications>();
        }

        private static IEnumerable<object[]> EmptyOrDisabledSettings()
        {
            return new List<object[]>
            {
                new[]{ CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Disabled) },
                new[]{ new CommandNotificationSettings[] { } }
            };
        }

        [Theory]
        [MemberData(nameof(EmptyOrDisabledSettings))]
        public async Task OnExecute_WhenAppCommandIsNotConfiguredToBeInSage_DoesNothing(CommandNotificationSettings[] settings)
        {
            var sut = SetupCommandHandlerCallbacks(settings);
            var appCmd = CreateTestCommand<TestCommand1>();

            await sut.OnExecute(appCmd);

            _mockSagaNotifications.Verify(f => f.RegisterNewSaga(appCmd.CommandContext.CorrelationId, appCmd.CommandContext.CommandId), Times.Never());
            _mockImmediateNotifications.Verify(f => f.RegisterNew(appCmd.CommandContext.CorrelationId, appCmd.CommandContext.CommandId), Times.Never());
        }

        [Fact]
        public async Task OnExecute_WhenAppCommandIsConfiguredToBeSagaInitiator_AddsInitiatorKeyToExtraDataAndRegistersNewSaga()
        {
            var commandNotificationSettings = CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Saga);
            var sut = SetupCommandHandlerCallbacks(commandNotificationSettings);
            var appCmd = CreateTestCommand<TestCommand1>();

            await sut.OnExecute(appCmd);

            AssertSagaInitiatorKey(appCmd);
            _mockSagaNotifications.Verify(f => f.RegisterNewSaga(appCmd.CommandContext.CorrelationId, appCmd.CommandContext.CommandId), Times.Once());
        }

        [Fact]
        public async Task OnExecute_WhenAppCommandIsConfiguredToBeSagaInitiator_AddsInitiatorKeyToExtraDataAndRegistersNewImmediateNotifications()
        {
            var commandNotificationSettings = CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Immediate);
            var sut = SetupCommandHandlerCallbacks(commandNotificationSettings);
            var appCmd = CreateTestCommand<TestCommand1>();

            await sut.OnExecute(appCmd);

            AssertSagaInitiatorKey(appCmd);
            _mockImmediateNotifications.Verify(f => f.RegisterNew(appCmd.CommandContext.CorrelationId, appCmd.CommandContext.CommandId), Times.Once());
        }

        [Fact]
        public async Task OnUowCommit_WhenAppCommandIsConfiguredToBeSagaCompleting_MarksSagaAsCompleted()
        {
            var appCmd = CreateTestCommand<TestCommand2>();
            appCmd.CommandContext.ExtraData[SagaInitiatorKey] = nameof(TestCommand1);
            var commandNotificationSettings = CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Saga, completionCommandNames: new[] { nameof(TestCommand2) });
            var sut = SetupCommandHandlerCallbacks(commandNotificationSettings);

            await sut.OnUowCommit(appCmd);

            _mockSagaNotifications.Verify(f => f.MarkSagaAsCompleted(appCmd.CommandContext.CorrelationId, null), Times.Once());
        }

        [Fact]
        public async Task OnUowCommit_WhenAppCommandIsConfiguredToBeSagaFailureCommand_MarksSagaAsFailed()
        {
            var appCmd = CreateTestCommand<TestCommand2>();
            appCmd.CommandContext.ExtraData[SagaInitiatorKey] = nameof(TestCommand1);
            var commandNotificationSettings = CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Saga, failureCommandNames: new[] { nameof(TestCommand2) });
            var sut = SetupCommandHandlerCallbacks(commandNotificationSettings);

            await sut.OnUowCommit(appCmd);

            _mockSagaNotifications.Verify(f => f.MarkSagaAsFailed(appCmd.CommandContext.CorrelationId, null), Times.Once());
        }

        [Fact]
        public async Task OnUowCommit_CommandHasNoSagaInitiatorKey_DoesNotMarkSagaAsCompleted()
        {
            var appCmd = CreateTestCommand<TestCommand2>();
            var commandNotificationSettings = CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Saga, completionCommandNames: new[] { nameof(TestCommand2) });
            var sut = SetupCommandHandlerCallbacks(commandNotificationSettings);

            await sut.OnUowCommit(appCmd);

            _mockSagaNotifications.Verify(f => f.MarkSagaAsCompleted(appCmd.CommandContext.CorrelationId, null), Times.Never());
        }

        [Fact]
        public async Task CallExtension_ValidNotificationsModeKeyAndDisabledValue_ChangesModeToDisabled()
        {
            var commandNotificationSettings = CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Saga);
            var sut = SetupCommandHandlerCallbacks(commandNotificationSettings);
            var appCmd = CreateTestCommand<TestCommand1>();

            await sut.CallExtension("ReadModelNotifications_NotificationsMode", "Disabled");
            await sut.OnExecute(appCmd);

            AssertHasNoSagaInitiatorKey(appCmd);
            _mockSagaNotifications.Verify(f => f.RegisterNewSaga(It.IsAny<CorrelationId>(), It.IsAny<CommandId>()), Times.Never());
        }

        [Fact]
        public async Task CallExtension_ValidNotificationsModeKeyAndSagaValue_ChangesModeValue()
        {
            var commandNotificationSettings = CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Disabled);
            var sut = SetupCommandHandlerCallbacks(commandNotificationSettings);
            var appCmd = CreateTestCommand<TestCommand1>();

            await sut.CallExtension("ReadModelNotifications_NotificationsMode", "Saga");
            await sut.OnExecute(appCmd);

            AssertSagaInitiatorKey(appCmd);
            _mockSagaNotifications.Verify(f => f.RegisterNewSaga(appCmd.CommandContext.CorrelationId, appCmd.CommandContext.CommandId), Times.Once());
        }

        [Fact]
        public async Task CallExtension_ValidNotificationsModeKeyAndImmediateValue_ChangesModeValue()
        {
            var commandNotificationSettings = CreateTestNotificationSettings<TestCommand1>(ReadModelNotificationsMode.Disabled);
            var sut = SetupCommandHandlerCallbacks(commandNotificationSettings);
            var appCmd = CreateTestCommand<TestCommand1>();

            await sut.CallExtension("ReadModelNotifications_NotificationsMode", "Immediate");
            await sut.OnExecute(appCmd);

            AssertSagaInitiatorKey(appCmd);
            _mockImmediateNotifications.Verify(f => f.RegisterNew(appCmd.CommandContext.CorrelationId, appCmd.CommandContext.CommandId), Times.Once());
        }

        private static void AssertSagaInitiatorKey(ICommandContextOwner appCmd)
        {
            appCmd.CommandContext.ExtraData[SagaInitiatorKey].Should().Be(nameof(TestCommand1));
        }

        private static void AssertHasNoSagaInitiatorKey(ICommandContextOwner appCmd)
        {
            appCmd.CommandContext.ExtraData.ContainsKey(SagaInitiatorKey).Should().Be(false);
        }

        private static CommandNotificationSettings[] CreateTestNotificationSettings<T>(ReadModelNotificationsMode notificationsMode, string[]? completionCommandNames = null, string[]? failureCommandNames = null)
        {
            return new[]{ new CommandNotificationSettings
            {
                CommandName = typeof(T).Name,
                NotificationsMode = notificationsMode,
                SagaCompletionCommandNames = completionCommandNames ?? Array.Empty<string>(),
                SagaFailureCommandNames = failureCommandNames ?? Array.Empty<string>(),
            }
            };
        }

        private ICommandHandlerCallbacks SetupCommandHandlerCallbacks(CommandNotificationSettings[] commandNotificationSettings)
        {
            var services = new ServiceCollection();
            services.AddReadModelNotifications((_) => _mockImmediateNotifications.Object, (_) => _mockSagaNotifications.Object, (_) => new StubNotificationReader(commandNotificationSettings),
                eventOutboxFactory: (_) => Mock.Of<IEventOutbox>());
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<ICommandHandlerCallbacks>();
        }

        private static AppCommand<T> CreateTestCommand<T>() where T : ICommand, new()
        {
            var cmd = new T();
            var appCmd = new AppCommand<T>
            {
                Command = cmd,
                CommandContext = new CommandContext(CommandId.CreateNew(), CorrelationId.CreateNew(), Guid.NewGuid(), false, false, typeof(T).Name, new Dictionary<string, string>())
            };
            return appCmd;
        }
    }
}