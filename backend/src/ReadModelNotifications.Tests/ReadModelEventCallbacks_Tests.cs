using Castle.Core.Logging;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReadModelNotifications.ImmediateNotifications;
using ReadModelNotifications.SagaNotifications;
using ReadModelNotifications.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ReadModelNotifications.Tests
{

    public class ReadModelEventCallbacks_Tests
    {
        public class StubAppEvent : Event
        {
            public StubAppEvent() : base("stubAppEvent")
            {
            }
        }

        private readonly Mock<IImmediateNotifications> _mockImmediateNotifications = new Mock<IImmediateNotifications>();
        private readonly Mock<ISagaNotifications> _mockSagaNotifications = new Mock<ISagaNotifications>();
        private readonly Mock<IAppEvent<StubAppEvent>> _mockAppEvent = new Mock<IAppEvent<StubAppEvent>>();
        private readonly IEventConsumerCallbacks _sut;

        public ReadModelEventCallbacks_Tests()
        {
            _sut = SetupEventConsumerCallbacks();
        }

        [Fact]
        public void Doesnt_complete_events_when_received_event_has_disabled_notifications_mode()
        {
            SetupCommandContextWithNotificationsMode(ReadModelNotificationsMode.Disabled);

            _sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(It.IsAny<CorrelationId>(), null), Times.Never());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Never());
        }

        [Fact]
        public void Doesnt_complete_events_by_default_when_received_event_has_invalid_notifications_mode()
        {
            SetupCommandContextWithInvalidNotificationsMode();

            _sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(It.IsAny<CorrelationId>(), null), Times.Never());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Never());
        }

        [Fact]
        public void Doesnt_complete_events_by_default_when_received_event_has_no_notifications_mode()
        {
            SetupCommandContextWithoutNotificationsMode();

            _sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(It.IsAny<CorrelationId>(), null), Times.Never());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Never());
        }

        [Fact]
        public void Completes_events_when_received_event_has_immediate_notifications_mode()
        {
            SetupCommandContextWithNotificationsMode(ReadModelNotificationsMode.Immediate);

            _sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(It.IsAny<CorrelationId>(), null), Times.Once());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Never());
        }

        [Fact]
        public void Completes_events_when_received_event_has_saga_notifications_mode()
        {
            SetupCommandContextWithNotificationsMode(ReadModelNotificationsMode.Saga);

            _sut.OnEventProcessed(_mockAppEvent.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).Wait();

            _mockImmediateNotifications.Verify(f => f.NotifyCompleted(It.IsAny<CorrelationId>(), null), Times.Never());
            _mockSagaNotifications.Verify(f => f.MarkEventAsHandled(It.IsAny<CorrelationId>(), It.IsAny<StubAppEvent>()), Times.Once());
        }

        private void SetupCommandContextWithNotificationsMode(ReadModelNotificationsMode mode)
        {
            var ctx = CommandContext.CreateNew("");
            ctx.ExtraData = new Dictionary<string, string>
            {
                {
                    "ReadModelNotificationsMode", mode switch {
                            ReadModelNotificationsMode.Immediate => "0",
                            ReadModelNotificationsMode.Saga => "1",
                            ReadModelNotificationsMode.Disabled => "2", 
                            _ => throw new NotImplementedException(),
                        }
                }
            };
            _mockAppEvent.SetupGet(g => g.CommandContext).Returns(ctx);
        }

        private void SetupCommandContextWithInvalidNotificationsMode()
        {
            var ctx = CommandContext.CreateNew("");
            ctx.ExtraData = new Dictionary<string, string>
            {
                {
                    "ReadModelNotificationsMode", "x"
                }
            };
            _mockAppEvent.SetupGet(g => g.CommandContext).Returns(ctx);
        }

        private void SetupCommandContextWithoutNotificationsMode()
        {
            var ctx = CommandContext.CreateNew("");
            ctx.ExtraData = new Dictionary<string, string>();
            _mockAppEvent.SetupGet(g => g.CommandContext).Returns(ctx);
        }

        private IEventConsumerCallbacks SetupEventConsumerCallbacks()
        {
            var services = new ServiceCollection();
            services.AddQueryReadModelNotificiations((_) => _mockImmediateNotifications.Object, (_) => _mockSagaNotifications.Object);
            services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IEventConsumerCallbacks>();
        }
    }
}
