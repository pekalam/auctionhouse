using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using Common.Application.Events;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMq.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Tests.Base.Mocks.Events;
using Xunit;

namespace Test.Adapter.RabbitMq.EventBus
{
    [Trait("Category", "Unit")]
    public class ErrorEventOutboxProcessor_Tests
    {
        Mock<IAdvancedBus> advancedBus;
        Mock<IErrorEventOutboxStore> mockErrorEventOutboxStore;

        [Fact]
        public async Task Sends_and_deleted_outbox_error_item()
        {
            var outboxItems = Enumerable.Range(0, 2).Select(i => new GivenErrorEventOutboxItem(0)
            .WithRoutingKey(i.ToString())
            .Build()).ToArray();

            var processor = GivenOutboxProcessor(1, outboxItems);

            var ctsSource = new CancellationTokenSource();
            await processor.ProcessEvents(ctsSource.Token);

            foreach (var outboxItem in outboxItems)
            {
                AssertDeleted(outboxItem, Times.Once());
                AssertMessageIsPublished(outboxItem, ctsSource, Times.Once());
            }

        }

        [Fact]
        public async Task Deletes_and_doesnt_publish_outbox_item_reaching_max_redelivery()
        {
            const int maxRedelivery = 2;
            var outboxItems = Enumerable.Range(0, 2).Select(i => new GivenErrorEventOutboxItem(maxRedelivery)
            .WithRoutingKey(i.ToString())
            .Build()).ToArray();
            var processor = GivenOutboxProcessor(maxRedelivery, outboxItems);

            var ctsSource = new CancellationTokenSource();
            await processor.ProcessEvents(ctsSource.Token);

            foreach (var outboxItem in outboxItems)
            {
                AssertDeleted(outboxItem, Times.Once());
                AssertMessageIsPublished(outboxItem, ctsSource, Times.Never());
            }
        }

        [Fact]
        public async Task Deletes_and_doesnt_publish_outbox_item_with_invalid_message()
        {
            const int maxRedelivery = 2;
            var outboxItems = Enumerable.Range(0, 2).Select(_ => new GivenErrorEventOutboxItem(maxRedelivery).WithInvalidMesasgeJson().Build()).ToArray();

            var processor = GivenOutboxProcessor(maxRedelivery, outboxItems);

            var ctsSource = new CancellationTokenSource();
            await processor.ProcessEvents(ctsSource.Token);

            foreach (var outboxItem in outboxItems)
            {
                AssertDeleted(outboxItem, Times.Once());
                AssertMessageIsPublished(outboxItem, ctsSource, Times.Never());
            }
        }

        private ErrorEventOutboxProcessor GivenOutboxProcessor(int maxRedelivery, ErrorEventOutboxItem[] outboxItems)
        {
            var serviceProvider = SetupServiceProvider(outboxItems);

            var processor = new ErrorEventOutboxProcessor(serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                new EventBusSettings
                {
                    MaxRedelivery = maxRedelivery
                }, Mock.Of<ILogger<ErrorEventOutboxProcessor>>());
            return processor;
        }

        private void AssertDeleted(ErrorEventOutboxItem item, Times times)
        {
            mockErrorEventOutboxStore.Verify(f => f.Delete(item), times);
        }

        private void AssertMessageIsPublished(ErrorEventOutboxItem outboxItem, CancellationTokenSource ctsSource, Times times)
        {
            advancedBus.Verify(f => f.PublishAsync(
                It.IsAny<IExchange>(),
                outboxItem.RoutingKey,
                true,
                It.IsAny<MessageProperties>(),
                It.IsAny<byte[]>(), ctsSource.Token), times);
        }

        private ServiceProvider SetupServiceProvider(ErrorEventOutboxItem[] outboxItems)
        {
            var services = new ServiceCollection();

            var mockItemsFinder = new Mock<IErrorEventOutboxUnprocessedItemsFinder>();
            mockItemsFinder.Setup(f => f.FindUnprocessed(It.IsAny<int>()))
                .Returns(outboxItems);

            mockErrorEventOutboxStore = new Mock<IErrorEventOutboxStore>();
            var rabbitMqEventBus = new Mock<IEasyMQBusInstance>();
            advancedBus = new Mock<IAdvancedBus>();
            var exchange = new Mock<IExchange>();
            var bus = new Mock<IBus>();
            bus.SetupGet(b => b.Advanced).Returns(advancedBus.Object);

            rabbitMqEventBus.SetupGet(p => p.Bus).Returns(bus.Object);
            rabbitMqEventBus.SetupGet(p => p.EventExchange).Returns(exchange.Object);

            services.AddSingleton(rabbitMqEventBus.Object);
            services.AddSingleton(mockErrorEventOutboxStore.Object);
            services.AddSingleton(rabbitMqEventBus.Object);
            services.AddSingleton(mockItemsFinder.Object);

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
