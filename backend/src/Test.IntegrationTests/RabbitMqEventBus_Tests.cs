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
        public Event Event { get; }

        public CommandContext CommandContext { get; set; }

        public TestAppEvent(Event @event, CommandContext commandContext)
        {
            Event = @event;
            CommandContext = commandContext;
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

    public class TestCommandBase : ICommand { }

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
        [Sequential]
        public void Published_event_gets_handled()
        {
            RollbackHandlerRegistry.ImplProvider = new TestImplProv();

            var failed = false;
            var sem = new SemaphoreSlim(0, 1);
            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                ConnectionString = TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
            }, Mock.Of<ILogger<RabbitMqEventBus>>());

            var ctx = CommandContext.CreateNew(Guid.NewGuid(), "test");
            var toPublish = new TestAppEvent(new TestEvent(), ctx);

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
            bus.InitSubscribers("Test.IntegrationTests", stubImplProvider.Object);

            bus.Publish(toPublish);

            if (!sem.Wait(TimeSpan.FromSeconds(60)))
                Assert.Fail();
            Assert.False(failed);
        }

        private bool CheckRollbackAppEvent(IAppEvent<Event> ev)
        {
            return ev.CommandContext.Name
                       .Equals(nameof(TestCommandBase)) && ev.CommandContext.CorrelationId.Value.Equals("123") && ev.Event.GetType()
                       .Equals(typeof(TestEvent));
        }

        [Test]
        [Sequential]
        public void RollbackHandler_gets_called_once()
        {
            int called = 0;
            var sem = new SemaphoreSlim(0, 1);
            var ctx = CommandContext.CreateNew(Guid.NewGuid(), nameof(TestCommandBase));
            ctx.CorrelationId = new CorrelationId("123");
            var cmd = new AppCommand<TestCommandBase> { Command = new TestCommandBase(), CommandContext = ctx };
            var toPublish = new TestAppEvent(new TestEvent(), ctx);
            var checkSuccess = false;
            var rollbackHandler = new TestCommandRollbackHandler(new Action<IAppEvent<Event>>(ev =>
            {
                called++;
                checkSuccess = CheckRollbackAppEvent(ev);
                sem.Release();
            }));
            rollbackHandler.Throws = true;

            RollbackHandlerRegistry.ImplProvider = new TestImplProv();
            RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(TestCommandBase), provider => rollbackHandler);

            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                ConnectionString = TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
            }, Mock.Of<ILogger<RabbitMqEventBus>>());

            var handler = new TestHandler(new AppEventRabbitMQBuilder(), null);
            handler.Throws = true;

            bus.InitSubscribers(new[]
            {
                new RabbitMqEventConsumerFactory(() => handler, "testEvent"),
            });

            bus.Publish(toPublish);

            if(!sem.Wait(TimeSpan.FromSeconds(60)))
                Assert.Fail();

            called.Should()
                .Be(1);
            checkSuccess.Should().BeTrue();
        }

    }
}
