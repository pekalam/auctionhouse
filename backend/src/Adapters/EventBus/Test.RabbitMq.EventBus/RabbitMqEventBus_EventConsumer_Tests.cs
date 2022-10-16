using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Tests.Base;
using Core.Common.Domain;
using Core.Query.EventHandlers;
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
    public class TestConsumerEvent : Event
    {
        public TestConsumerEvent() : base("testEvent")
        {
        }
    }

    public class TestEventConsumer : EventConsumer<TestConsumerEvent, TestEventConsumer>
    {
        public static Action<IAppEvent<TestConsumerEvent>> OnConsume = delegate { };

        public TestEventConsumer(EventConsumerDependencies dependencies) : base(Mock.Of<ILogger<TestEventConsumer>>(), dependencies)
        {
        }

        public override Task Consume(IAppEvent<TestConsumerEvent> appEvent)
        {
            OnConsume(appEvent);
            return Task.CompletedTask;
        }
    }

    [Trait("Category", "Integration")]
    public class RabbitMqEventBus_EventConsumer_Tests : IDisposable
    {
        private Task? _hostTask;
        private IEventBus _bus;

        public RabbitMqEventBus_EventConsumer_Tests()
        {
            var host = Host.CreateDefaultBuilder().ConfigureServices((ctx, services) =>
            {
                services.AddEventConsumers(typeof(TestEventConsumer));
                services.AddTransient<IImplProvider, ImplProviderMock>();
                new CommonApplicationMockInstaller(services)
                    .AddQueryCoreDependencies(new[] { Assembly.Load("Test.Adapter.RabbitMq.EventBus") })
                    .AddRabbitMqAppEventBuilderAdapter()
                    .AddRabbitMqEventBusAdapter(rabbitMqSettings: TestConfig.Instance.GetRabbitMqSettings(), eventConsumerAssemblies: new[] { Assembly.Load("Test.Adapter.RabbitMq.EventBus") });
            }).Build();
            _hostTask = host.StartAsync();
            _bus = host.Services.GetRequiredService<IEventBus>();
        }

        [Fact]
        public async Task Published_event_gets_handled_by_EventConsumer()
        {
            var failed = false;
            var sem = new SemaphoreSlim(0, 1);

            var ctx = CommandContext.CreateNew("test", Guid.NewGuid());
            var toPublish = new AppEventRabbitMQBuilder()
                .WithCommandContext(ctx)
                .WithEvent(new TestConsumerEvent())
                .Build<TestConsumerEvent>();

            TestEventConsumer.OnConsume += (ev) =>
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