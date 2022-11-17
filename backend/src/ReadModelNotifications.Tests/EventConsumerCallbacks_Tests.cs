using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReadModelNotifications.ImmediateNotifications;
using ReadModelNotifications.SagaNotifications;
using System;
using System.Collections.Generic;
using Xunit;

namespace ReadModelNotifications.Tests
{

    public class EventConsumerCallbacks_Tests
    {
        private const string SagaInitiatorExtraKey = "SagaInitiator";

        public class StubAppEvent : Event
        {
            public StubAppEvent() : base("StubAppEvent")
            {
            }
        }

        private readonly Mock<IImmediateNotifications> _mockImmediateNotifications = new Mock<IImmediateNotifications>();
        private readonly Mock<ISagaNotifications> _mockSagaNotifications = new Mock<ISagaNotifications>();
        private readonly Mock<IAppEvent<StubAppEvent>> _mockAppEvent = new Mock<IAppEvent<StubAppEvent>>();

        [Fact]
        public void Doesnt_complete_events_when_received_event_has_disabled_notifications_mode()
        {
            var sut = SetupCommandContextWithNotificationsMode(ReadModelNotificationsMode.Disabled);

            sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(It.IsAny<CorrelationId>(), null), Times.Never());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Never());
        }

        [Fact]
        public void Doesnt_complete_events_by_default_when_received_event_has_no_notifications_mode()
        {
            var sut = SetupCommandContextWithoutNotificationsMode();

            sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(It.IsAny<CorrelationId>(), null), Times.Never());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Never());
        }

        [Fact]
        public void Completes_events_when_received_event_has_immediate_notifications_mode()
        {
            var sut = SetupCommandContextWithNotificationsMode(ReadModelNotificationsMode.Immediate);

            sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(_mockAppEvent.Object.CommandContext.CorrelationId, null), Times.Once());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Never());
        }

        [Fact]
        public void Completes_events_when_received_event_has_immediate_notifications_mode_without_confirmation_event()
        {
            var sut = SetupCommandContextWithImmediateNotificationsModeWithoutConfrimationEvent();

            sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(_mockAppEvent.Object.CommandContext.CorrelationId, null), Times.Once());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Never());
        }

        [Fact]
        public void Completes_events_when_received_event_has_saga_notifications_mode()
        {
            var sut = SetupCommandContextWithNotificationsMode(ReadModelNotificationsMode.Saga);

            sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(It.IsAny<CorrelationId>(), null), Times.Never());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(_mockAppEvent.Object.CommandContext.CorrelationId, 
                _mockAppEvent.Object.Event), Times.Once());
        }

        private IEventConsumerCallbacks SetupCommandContextWithImmediateNotificationsModeWithoutConfrimationEvent()
        {
            var provider = SetupDIAndConfiguration(SetupImmediateConfigurationWithoutConfirmationEvent());

            var ctx = CommandContext.CreateNew("Test1Cmd");
            ctx.ExtraData[SagaInitiatorExtraKey] = "Test1Cmd";
            _mockAppEvent.SetupGet(g => g.CommandContext).Returns(ctx);
            _mockAppEvent.SetupGet(g => g.Event).Returns(new StubAppEvent());

            return provider.GetRequiredService<IEventConsumerCallbacks>();
        }

        private IEventConsumerCallbacks SetupCommandContextWithNotificationsMode(ReadModelNotificationsMode mode)
        {
            var provider = SetupDIAndConfiguration(mode switch
            {
                ReadModelNotificationsMode.Saga => SetupSagaConfiguration(),
                ReadModelNotificationsMode.Immediate => SetupImmediateConfiguration(),
                ReadModelNotificationsMode.Disabled => SetupDisabledConfiguration(),
                _ => throw new InvalidOperationException()
            });

            var ctx = CommandContext.CreateNew("Test1Cmd");
            ctx.ExtraData[SagaInitiatorExtraKey] = "Test1Cmd";
            _mockAppEvent.SetupGet(g => g.CommandContext).Returns(ctx);
            _mockAppEvent.SetupGet(g => g.Event).Returns(new StubAppEvent());

            return provider.GetRequiredService<IEventConsumerCallbacks>();
        }

        private IEventConsumerCallbacks SetupCommandContextWithoutNotificationsMode()
        {
            var provider = SetupDIAndConfiguration(SetupSagaConfiguration());

            var ctx = CommandContext.CreateNew("");
            ctx.ExtraData = new Dictionary<string, string>();
            _mockAppEvent.SetupGet(g => g.CommandContext).Returns(ctx);
            _mockAppEvent.SetupGet(g => g.Event).Returns(new StubAppEvent());

            return provider.GetRequiredService<IEventConsumerCallbacks>();
        }

        private ServiceProvider SetupDIAndConfiguration(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            services.AddQueryReadModelNotificiations((_) => _mockImmediateNotifications.Object, (_) => _mockSagaNotifications.Object,
                configuration, null);
            services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            var provider = services.BuildServiceProvider();
            return provider;
        }

        private static IConfiguration SetupSagaConfiguration()
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1Cmd"},
                {"CommandNotificationSettings:0:NotificationsMode", "Saga"},
                {"CommandNotificationSettings:0:EventsToConfirm:0", "StubAppEvent"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:0", "Completion2"},
            });
            return config.Build();
        }

        private static IConfiguration SetupImmediateConfiguration()
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1Cmd"},
                {"CommandNotificationSettings:0:NotificationsMode", "Immediate"},
                {"CommandNotificationSettings:0:EventsToConfirm:0", "StubAppEvent"},
            });
            return config.Build();
        }

        private static IConfiguration SetupImmediateConfigurationWithoutConfirmationEvent()
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1Cmd"},
                {"CommandNotificationSettings:0:NotificationsMode", "Immediate"},
            });
            return config.Build();
        }

        private static IConfiguration SetupDisabledConfiguration()
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1Cmd"},
                {"CommandNotificationSettings:0:NotificationsMode", "Disabled"},
            });
            return config.Build();
        }
    }
}
