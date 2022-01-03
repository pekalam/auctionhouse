using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMq.EventBus;
using System;
using System.Reflection;
using System.Threading;
using Xunit;

namespace Test.RabbitMq.EventBus
{
    public class TestEvent : Event
    {
        public TestEvent() : base("testEvent")
        {
        }
    }

    public class TestAppEvent : IAppEvent<Event>
    {
        public Event Event { get; }

        public CommandContext CommandContext { get; set; }

        public ReadModelNotificationsMode ReadModelNotifications { get; set; }

        public TestAppEvent(Event @event, CommandContext commandContext, ReadModelNotificationsMode consistencyMode)
        {
            Event = @event;
            CommandContext = commandContext;
            ReadModelNotifications = consistencyMode;
        }
    }

    public class TestHandler : EventConsumer<TestEvent>
    {
        private Action<IAppEvent<TestEvent>> OnConsume;
        public bool Throws { get; set; }

        public TestHandler(IAppEventBuilder appEventBuilder, Action<IAppEvent<TestEvent>> onConsume) : base(appEventBuilder, Mock.Of<ILogger<TestHandler>>(), () => Mock.Of<ISagaNotifications>())
        {
            OnConsume = onConsume;
        }

        public override void Consume(IAppEvent<TestEvent> appEvent)
        {
            OnConsume?.Invoke(appEvent);
            if (Throws)
            {
                throw new Exception();
            }
        }
    }

    public class RabbitMqEventBus_EventConsumer_Tests
    {
        [Fact]
        public void Published_event_gets_handled_by_EventConsumer()
        {
            var failed = false;
            var sem = new SemaphoreSlim(0, 1);
            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                //ConnectionString = TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
                ConnectionString = "host=localhost",
            }, Mock.Of<ILogger<RabbitMqEventBus>>());

            var ctx = CommandContext.CreateNew(Guid.NewGuid(), "test");
            var toPublish = new TestAppEvent(new TestEvent(), ctx, ReadModelNotificationsMode.Immediate);

            var handler = new TestHandler(new AppEventRabbitMQBuilder(), (ev) =>
            {
                try
                {
                    ev.Should().BeEquivalentTo(toPublish);
                }
                catch (Exception ex)
                {
                    failed = true;
                }
                sem.Release();
            });


            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(provider => provider.Get(typeof(TestHandler)))
            .Returns(handler);
            
            bus.InitEventConsumers(stubImplProvider.Object, Assembly.Load("Test.RabbitMq.EventBus"));

            bus.Publish(toPublish);

            if (!sem.Wait(TimeSpan.FromSeconds(60)))
                Assert.False(true);
            Assert.False(failed);
        }
    }
}