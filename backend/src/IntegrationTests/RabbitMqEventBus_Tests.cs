using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Core.Command;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;
using Core.Query.EventHandlers;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Services.EventBus;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IntegrationTests
{
    public class TestEvent : Event
    {
        public TestEvent() : base("testEvent")
        {
        }
    }

    public class TestAppEvent : IAppEvent<Event>
    {
        public ICommand Command { get; } = null;
        public CorrelationId CorrelationId { get; }
        public Event Event { get; }


        public TestAppEvent(CorrelationId correlationId, Event @event, ICommand cmd = null)
        {
            CorrelationId = correlationId;
            Event = @event;
            Command = cmd;
        }
    }

    public class TestHandler : EventConsumer<TestEvent>
    {
        private Action<IAppEvent<TestEvent>> OnConsume;
        public bool Throws { get; set; }

        public TestHandler(IAppEventBuilder appEventBuilder, Action<IAppEvent<TestEvent>> onConsume) : base(appEventBuilder, Mock.Of<ILogger<TestHandler>>())
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

    public class TestImplProv : IImplProvider
    {
        public T Get<T>() where T : class
        {
            return Mock.Of<T>();
        }

        public object Get(Type t)
        {
            return Mock.Get(t).Object;
        }
    }

    public class TestCommand : ICommand { }

    public class TestCommandRollbackHandler : ICommandRollbackHandler
    {
        private Action<IAppEvent<Event>> OnRollback;
        public bool Throws { get; set; }

        public TestCommandRollbackHandler(Action<IAppEvent<Event>> onRollback)
        {
            OnRollback = onRollback;
        }

        public virtual void Rollback(IAppEvent<Event> commandEvent)
        {
            OnRollback?.Invoke(commandEvent);
            if (Throws)
            {
                throw new Exception();
            }
        }
    }

    [TestFixture]
    public class RabbitMqEventBus_Tests
    {
        [Test]
        public void Published_event_gets_handled()
        {
            RollbackHandlerRegistry.ImplProvider = new TestImplProv();

            var sem = new SemaphoreSlim(0, 1);
            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                ConnectionString = TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
            }, Mock.Of<ILogger<RabbitMqEventBus>>());

            var toPublish = new TestAppEvent(new CorrelationId("111"), new TestEvent());

            var handler = new TestHandler(new AppEventRabbitMQBuilder(), (ev) =>
            {
                ev.Should().BeEquivalentTo(toPublish);
                sem.Release();
            });


            var stubImplProvider = new Mock<IImplProvider>();
                stubImplProvider.Setup(provider => provider.Get(typeof(TestHandler)))
                .Returns(handler);
            bus.InitSubscribers("IntegrationTests", stubImplProvider.Object);

            bus.Publish(toPublish);

            if (!sem.Wait(TimeSpan.FromSeconds(60)))
            {
                Assert.Fail();
            }
        }

        private bool CheckRollbackAppEvent(IAppEvent<Event> ev)
        {
            return ev.Command.GetType()
                       .Equals(typeof(TestCommand)) && ev.CorrelationId.Value.Equals("123") && ev.Event.GetType()
                       .Equals(typeof(TestEvent));
        }

        [Test]
        public void RollbackHandler_gets_called_once()
        {
            int called = 0;
            var sem = new SemaphoreSlim(0, 1);
            var toPublish = new TestAppEvent(new CorrelationId("123"), new TestEvent(), new TestCommand());
            var rollbackHandler = new TestCommandRollbackHandler(new Action<IAppEvent<Event>>(ev =>
            {
                called++;
                Assert.IsTrue(CheckRollbackAppEvent(ev));
                sem.Release();
            }));
            rollbackHandler.Throws = true;

            RollbackHandlerRegistry.ImplProvider = new TestImplProv();
            RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(TestCommand), provider => rollbackHandler);

            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                ConnectionString = TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
            }, Mock.Of<ILogger<RabbitMqEventBus>>());

            var handler = new TestHandler(new AppEventRabbitMQBuilder(), null);
            handler.Throws = true;

            bus.InitSubscribers(new[]
            {
                new RabbitMqEventConsumerFactory(() => handler, "test"),
            });

            bus.Publish(toPublish);

            if(!sem.Wait(TimeSpan.FromSeconds(60))){Assert.Fail();};
            Thread.Sleep(10);

            called.Should()
                .Be(1);
        }

    }
}
