using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.DependencyInjection;
using Common.Tests.Base;
using Core.Common.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RabbitMq.EventBus;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Test.Adapter.RabbitMq.EventBus;
using TestConfigurationAccessor;
using Xunit;

namespace Test.RabbitMq.EventBus
{
    public class TestSubscriberEvent : Event
    {
        public TestSubscriberEvent() : base("testSubEvent")
        {
        }
    }

    public class TestEventSubscriber : EventSubscriber<TestSubscriberEvent>
    {
        public static event Action<IAppEvent<TestSubscriberEvent>> OnConsume = delegate { };

        public TestEventSubscriber(IAppEventBuilder appEventBuilder) : base(appEventBuilder)
        {
        }

        public override Task Handle(IAppEvent<TestSubscriberEvent> appEvent)
        {
            OnConsume?.Invoke(appEvent);
            return Task.CompletedTask;
        }
    }

    [Trait("Category", "Integration")]
    public class RabbitMqEventBus_EventSubscriber_Tests : IDisposable
    {
        private Task? _hostTask;
        private IEventBus _bus;

        public RabbitMqEventBus_EventSubscriber_Tests()
        {
            var host = Host.CreateDefaultBuilder().ConfigureServices((ctx, services) =>
            {
                services.AddEventSubscribers(typeof(TestEventSubscriber));
                services.AddTransient<IImplProvider, ImplProviderMock>();
                new CommonApplicationMockInstaller(services)
                    .AddRabbitMqAppEventBuilderAdapter()
                    .AddRabbitMqEventBusAdapter(rabbitMqSettings: TestConfig.Instance.GetRabbitMqSettings(), eventSubscriptionAssemblies: new[] { Assembly.Load("Test.Adapter.RabbitMq.EventBus") });
            }).Build();
            _hostTask = host.StartAsync();
            _bus = host.Services.GetRequiredService<IEventBus>();
        }

        [Fact]
        public async Task Published_event_gets_handled_by_EventSubscriber()
        {
            var failed = false;
            var sem = new SemaphoreSlim(0, 1);

            var ctx = CommandContext.CreateNew("test", Guid.NewGuid());
            var toPublish = new AppEventRabbitMQBuilder()
                .WithCommandContext(ctx)
                .WithEvent(new TestSubscriberEvent())
                .Build<TestSubscriberEvent>();

            TestEventSubscriber.OnConsume += (ev) =>
            {
                try
                {
                    ev.Should().BeEquivalentTo(toPublish);
                }
                catch (Exception)
                {
                    failed = true;
                }
                sem.Release();
            };

            await _bus.Publish(toPublish);

            if (!sem.Wait(TimeSpan.FromSeconds(60)))
                Assert.False(true);
            Assert.False(failed);
        }

        public void Dispose()
        {
            _hostTask?.Wait();
        }
    }
}