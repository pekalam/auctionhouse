﻿using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using Common.Application;
using Common.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMq.EventBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test.Common.Base.Scenarios;
using Xunit;

namespace Test.Adapter.RabbitMq.EventBus
{
    [CollectionDefinition(nameof(RedeliveryTestCollection), DisableParallelization = true)]
    public class RedeliveryTestCollection { }


    public class RabbitMqEventBusRedeliveryTestBase
    {
        protected static IImplProvider SetupImplProvider(EventBusRedeliveryTestEventConsumer handler)
        {
            var services = new ServiceCollection();
            services.AddSingleton(handler);
            return AddAdapterServices(services);
        }

        protected static IImplProvider AddAdapterServices(ServiceCollection services)
        {
            services.AddSingleton(Mock.Of<ILogger<RabbitMqEventBus>>());
            services.AddSingleton(Mock.Of<ILogger<ErrorEventOutboxProcessor>>());
            services.AddRabbitMqEventBus(new()
            {
                ConnectionString = "host=localhost",
            });
            services.AddErrorEventOutbox(new()
            {
                DatabasePath = @".\testDb"
            });
            var serviceProvider = services.BuildServiceProvider();
            var stubImplProvider = new ImplProviderMock(serviceProvider);
            return stubImplProvider;
        }

        protected static IImplProvider SetupImplProvider(EventBusRedeliveryTestEventSubscriber handler)
        {
            var services = new ServiceCollection();
            services.AddSingleton(handler);
            return AddAdapterServices(services);
        }
    }


    [Collection(nameof(RedeliveryTestCollection))]
    public class RabbitMqEventBus_EventConsumer_Redelivery_Tests : RabbitMqEventBusRedeliveryTestBase, IDisposable
    {
        const string TestDbPath = @".\testDb";
        EventBusRedeliveryAdapterConsumerScenario scenario;
        RabbitMqEventBus bus;
        IImplProvider stubImplProvider;

        public RabbitMqEventBus_EventConsumer_Redelivery_Tests()
        {
            scenario = EventBusRedeliveryAdapterContract.EventConsumerRedeliveryScenario;
            stubImplProvider = SetupImplProvider(scenario.given.eventConsumer);
            bus = stubImplProvider.Get<RabbitMqEventBus>();
            bus.InitEventConsumers(stubImplProvider, Assembly.Load("Test.Common.Base"));
            bus.SetupErrorQueueSubscribtion();
        }

        public void Dispose()
        {
            bus.Dispose();
            RocksDbErrorEventOutboxStorage.db.Value.Dispose();
            Directory.Delete(TestDbPath, true);
        }

        [Fact]
        public async Task Redelivers_message_to_consumers_expecte_number_of_times()
        {
            bus.Publish(scenario.given.eventToSend);

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
        const string TestDbPath = @".\testDb";
        EventBusRedeliveryAdapterSubscriberScenario scenario;
        RabbitMqEventBus bus;
        IImplProvider stubImplProvider;

        public RabbitMqEventBus_Subscriber_Redelivery_Test()
        {
            scenario = EventBusRedeliveryAdapterContract.EventSubscriberRedeliveryScenario;
            stubImplProvider = SetupImplProvider(scenario.given.eventSubscriber);
            bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                //ConnectionString = TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
                ConnectionString = "host=localhost",
            }, Mock.Of<ILogger<RabbitMqEventBus>>(), stubImplProvider.Get<IServiceScopeFactory>());

            bus.InitEventSubscriptions(stubImplProvider, Assembly.Load("Test.Common.Base"));
            bus.SetupErrorQueueSubscribtion();
        }

        public void Dispose()
        {
            bus.Dispose();
            RocksDbErrorEventOutboxStorage.db.Value.Dispose();
            Directory.Delete(TestDbPath, true);
        }


        [Fact]
        public async Task Redelivers_message_to_subscriber_expecte_number_of_times()
        {
            bus.Publish(scenario.given.eventToSend);

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