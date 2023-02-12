using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReadModelNotifications.ImmediateNotifications;
using ReadModelNotifications.SagaNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ReadModelNotifications.Tests
{
    public class CommandNotificationsEventOutboxWrapper_Tests
    {
        private const string NotificationsModeExtraKey = nameof(ReadModelNotificationsMode);
        private const string SagaInitiatorExtraKey = "SagaInitiator";

        [Fact]
        public async Task Adds_unhandled_events_when_command_is_configured_with_saga_notifications_mode()
        {
            var mockSagaNotifications = new Mock<ISagaNotifications>();
            var eventOutbox = SetupEventOutbox(mockSagaNotifications);
            var events = new[] { (Event)new Test1Event(), new Test2Event() };
            var eventsToConfirm = events.Where(e => e is Test1Event);
            var ctx = CommandContext.CreateNew("Test1Cmd");
            ctx.ExtraData[SagaInitiatorExtraKey] = "Test1Cmd";

            var items = await eventOutbox.SaveEvents(events, ctx);

            mockSagaNotifications.Verify(f => f.AddUnhandledEvents(ctx.CorrelationId,
                It.Is<IEnumerable<Event>>(e => e.Except(eventsToConfirm).Count() == 0)), Times.Once());
        }

        private static IEventOutbox SetupEventOutbox(Mock<ISagaNotifications> mockSagaNotifications)
        {
            var config = SetupConfiguration();

            var services = new ServiceCollection();
            services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            services.AddCommandReadModelNotifications(
                (_) => Mock.Of<IImmediateNotifications>(),
                (_) => mockSagaNotifications.Object,
                config,
                eventOutboxFactoryTestDependency: (_) => new StubEventOutbox());
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IEventOutbox>();
        }

        private static IConfigurationRoot SetupConfiguration()
        {
            var config = new ConfigurationBuilder();

            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"CommandNotificationSettings:0:CommandName", "Test1Cmd"},
                {"CommandNotificationSettings:0:NotificationsMode", "Saga"},
                {"CommandNotificationSettings:0:EventsToConfirm:0", "Test1"},
                {"CommandNotificationSettings:0:SagaCompletionCommandNames:0", "Completion2"},
            });
            return config.Build();
        }

        private class Test1Event : Event
        {
            public Test1Event() : base("Test1")
            {
            }
        }

        private class Test2Event : Event
        {
            public Test2Event() : base("Test2")
            {
            }
        }

        public class StubEventOutbox : IEventOutbox
        {
            public IReadOnlyList<OutboxItem> SavedOutboxStoreItems => throw new NotImplementedException();

            public Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext)
            {
                throw new NotImplementedException();
            }

            public async Task<OutboxItem[]> SaveEvents(IEnumerable<Event> @event, CommandContext commandContext)
            {
                return @event.Select(e => new OutboxItem() { CommandContext = commandContext, Event = e, }).ToArray();
            }
        }
    }


}
