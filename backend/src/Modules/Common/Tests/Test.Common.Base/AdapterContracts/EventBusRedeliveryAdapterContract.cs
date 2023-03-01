using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Tests.Base.Mocks.Events;
using Common.Application.Events.Callbacks;

namespace Common.Tests.Base.AdapterContracts
{
    public class EventBusRedeliveryTestEventSubscriber : EventSubscriber<EventBusRedeliveryEvent>
    {
        internal static int RedeliveryCount = -1;
        internal static string correlationId;

        public EventBusRedeliveryTestEventSubscriber(IAppEventBuilder eventBuilder) : base(eventBuilder)
        {
        }

        public override Task Handle(IAppEvent<EventBusRedeliveryEvent> appEvent)
        {
            if (correlationId == null)
            {
                correlationId = appEvent.CommandContext.CorrelationId.Value;
            }

            RedeliveryCount = appEvent.CommandContext.CorrelationId.Value == correlationId ? appEvent.RedeliveryCount : RedeliveryCount;
            if (RedeliveryCount != 3)
            {
                throw new Exception();
            }
            return Task.CompletedTask;
        }
    }

    public class EventBusRedeliveryTestEventConsumer : EventConsumer<EventBusRedeliveryEvent, EventBusRedeliveryTestEventConsumer>
    {
        internal static int RedeliveryCount = -1;
        internal static string correlationId;

        public EventBusRedeliveryTestEventConsumer(ILogger<EventBusRedeliveryTestEventConsumer> logger, EventConsumerDependencies dependencies) : base(logger, dependencies)
        {
        }

        public override Task Consume(IAppEvent<EventBusRedeliveryEvent> appEvent)
        {
            if (correlationId == null)
            {
                correlationId = appEvent.CommandContext.CorrelationId.Value;
            }

            RedeliveryCount = appEvent.CommandContext.CorrelationId.Value == correlationId ? appEvent.RedeliveryCount : RedeliveryCount;
            if (RedeliveryCount != 3)
            {
                throw new Exception();
            }
            return Task.CompletedTask;
        }
    }

    public class EventBusRedeliveryEvent : Event
    {
        public EventBusRedeliveryEvent() : base("eventBusRedeliveryEvent")
        {
        }
    }

    public record EventBusRedeliveryAdapterSubscriberScenario((EventBusRedeliveryTestEventSubscriber eventSubscriber, IAppEvent<EventBusRedeliveryEvent> eventToSend) given, Action expectedNotThrowingAssertion);
    public record EventBusRedeliveryAdapterConsumerScenario((EventBusRedeliveryTestEventConsumer eventConsumer, IAppEvent<EventBusRedeliveryEvent> eventToSend) given, Action expectedNotThrowingAssertion);

    public static class EventBusRedeliveryAdapterContract
    {
        public static EventBusRedeliveryAdapterSubscriberScenario EventSubscriberRedeliveryScenario
        {
            get
            {
                var appEventBuilder = new TestAppEventBuilder();
                var cmdContext = CommandContext.CreateNew("test");
                return new(new(new EventBusRedeliveryTestEventSubscriber(appEventBuilder), appEventBuilder.WithCommandContext(cmdContext)
                .WithEvent(new EventBusRedeliveryEvent())
                .Build<EventBusRedeliveryEvent>()), () =>
                {
                    if (EventBusRedeliveryTestEventSubscriber.RedeliveryCount != 3)
                    {
                        throw new Exception("Invalid redelivery count " + EventBusRedeliveryTestEventSubscriber.RedeliveryCount);
                    }
                });
            }
        }

        public static EventBusRedeliveryAdapterConsumerScenario EventConsumerRedeliveryScenario
        {
            get
            {
                var appEventBuilder = new TestAppEventBuilder();
                var cmdContext = CommandContext.CreateNew("test");
                return new(new(new(Mock.Of<ILogger<EventBusRedeliveryTestEventConsumer>>(), new EventConsumerDependencies(new TestAppEventBuilder(), Mock.Of<IEventConsumerCallbacks>())), appEventBuilder.WithCommandContext(cmdContext)
                .WithEvent(new EventBusRedeliveryEvent())
                .Build<EventBusRedeliveryEvent>()), () =>
                {
                    if (EventBusRedeliveryTestEventConsumer.RedeliveryCount != 3)
                    {
                        throw new Exception("Invalid redelivery count " + EventBusRedeliveryTestEventConsumer.RedeliveryCount);
                    }
                });
            }
        }
    }
}
