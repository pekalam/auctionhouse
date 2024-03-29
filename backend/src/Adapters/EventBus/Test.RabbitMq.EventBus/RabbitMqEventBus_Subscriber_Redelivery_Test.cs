﻿using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using Common.Application;
using Common.Application.Events;
using Common.Tests.Base.AdapterContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMq.EventBus;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TestConfigurationAccessor;
using Xunit;

namespace Test.Adapter.RabbitMq.EventBus
{
    [CollectionDefinition(nameof(RedeliveryTestCollection), DisableParallelization = true)]
    public class RedeliveryTestCollection { }

    [Trait("Category", "Integration")]
    public class RabbitMqEventBusRedeliveryTestBase : IDisposable
    {
        private readonly RocksDbFixture _dbFixture = new RocksDbFixture();

        protected IImplProvider SetupImplProvider(EventBusRedeliveryTestEventConsumer handler)
        {
            var services = new ServiceCollection();
            services.AddLogging(b => b.AddXUnit());
            services.AddSingleton(handler);
            return AddAdapterServices(services);
        }

        protected IImplProvider AddAdapterServices(ServiceCollection services)
        {
            services.AddRabbitMqEventBus(TestConfig.Instance.GetRabbitMqSettings());
            services.AddErrorEventOutbox(new()
            {
                DatabasePath = _dbFixture.TestDbPath,
            });
            var serviceProvider = services.BuildServiceProvider();
            var stubImplProvider = new ImplProviderMock(serviceProvider);
            return stubImplProvider;
        }

        protected IImplProvider SetupImplProvider(EventBusRedeliveryTestEventSubscriber handler)
        {
            var services = new ServiceCollection();
            services.AddLogging(b => b.AddXUnit());
            services.AddSingleton(handler);
            return AddAdapterServices(services);
        }

        public void Dispose()
        {
            _dbFixture.Dispose();
        }
    }


    [Collection(nameof(RedeliveryTestCollection))]
    public class RabbitMqEventBus_EventConsumer_Redelivery_Tests : RabbitMqEventBusRedeliveryTestBase, IDisposable
    {
        EventBusRedeliveryAdapterConsumerScenario scenario;
        RabbitMqEventBus bus;
        IImplProvider stubImplProvider;

        public RabbitMqEventBus_EventConsumer_Redelivery_Tests()
        {
            scenario = EventBusRedeliveryAdapterContract.EventConsumerRedeliveryScenario;
            stubImplProvider = SetupImplProvider(scenario.given.eventConsumer);
            bus = stubImplProvider.Get<RabbitMqEventBus>();
            stubImplProvider.Get<IEasyMQBusInstance>().InitEventConsumers(stubImplProvider, Assembly.Load("Common.Tests.Base"));
            bus.SetupErrorQueueSubscribtion();
        }

        public void Dispose()
        {
            stubImplProvider.Get<IEasyMQBusInstance>().Dispose();
            RabbitMqUtils.PurgeQueues();
        }

        [Fact]
        public async Task Redelivers_message_to_consumers_expecte_number_of_times()
        {
            await bus.Publish(scenario.given.eventToSend);

            var processor = new ErrorEventOutboxProcessor(stubImplProvider.Get<IServiceScopeFactory>(), new EventBusSettings
            {
                MaxRedelivery = 3,
            }, stubImplProvider.Get<ILogger<ErrorEventOutboxProcessor>>());

            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(3_000);
                await processor.ProcessEvents(CancellationToken.None);
            }
            await Task.Delay(3_000);

            scenario.expectedNotThrowingAssertion();
        }
    }


    [Collection(nameof(RedeliveryTestCollection))]
    public class RabbitMqEventBus_Subscriber_Redelivery_Test : RabbitMqEventBusRedeliveryTestBase, IDisposable
    {
        EventBusRedeliveryAdapterSubscriberScenario scenario;
        RabbitMqEventBus bus;
        IImplProvider stubImplProvider;

        public RabbitMqEventBus_Subscriber_Redelivery_Test()
        {
            scenario = EventBusRedeliveryAdapterContract.EventSubscriberRedeliveryScenario;
            stubImplProvider = SetupImplProvider(scenario.given.eventSubscriber);
            bus = new RabbitMqEventBus(TestConfig.Instance.GetRabbitMqSettings(), stubImplProvider.Get<ILogger<RabbitMqEventBus>>(), stubImplProvider.Get<IEasyMQBusInstance>());

            stubImplProvider.Get<IEasyMQBusInstance>().InitEventSubscriptions(stubImplProvider, Assembly.Load("Common.Tests.Base"));
            bus.SetupErrorQueueSubscribtion();
        }

        public void Dispose()
        {
            stubImplProvider.Get<IEasyMQBusInstance>().Dispose();
            RabbitMqUtils.PurgeQueues();
        }


        [Fact]
        public async Task Redelivers_message_to_subscriber_expecte_number_of_times()
        {
            await bus.Publish(scenario.given.eventToSend);

            var processor = new ErrorEventOutboxProcessor(stubImplProvider.Get<IServiceScopeFactory>(), new EventBusSettings
            {
                MaxRedelivery = 3,
            }, stubImplProvider.Get<ILogger<ErrorEventOutboxProcessor>>());

            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(3_000);
                await processor.ProcessEvents(CancellationToken.None);
            }
            await Task.Delay(3_000);

            scenario.expectedNotThrowingAssertion();
        }
    }

    internal class ImplProviderMock : IImplProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ImplProviderMock(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Get<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        public object Get(Type t)
        {
            return _serviceProvider.GetService(t);
        }
    }
}
