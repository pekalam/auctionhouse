using Common.Application;
using Core.DomainFramework;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace RabbitMq.EventBus
{
    internal interface IEasyMQBusInstance : IDisposable
    {
        IBus Bus { get; }
        IExchange EventExchange { get; }
        void InitEventSubscriptions(IImplProvider implProvider, params Assembly[] assemblies);
        void InitEventConsumers(IImplProvider implProvider, params Assembly[] assemblies);
    }

    internal class EasyMQBusInstance : IEasyMQBusInstance
    {
        private readonly RabbitMqSettings _settings;
        private readonly RabbitMqEventBusSubscriptions _eventSubscriptions;
        private readonly RabbitMqEventBusSubscriptions _eventConsumers;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<EasyMQBusInstance> _logger;

        public IBus Bus { get; private set; }
        public IExchange EventExchange { get; }

        public EasyMQBusInstance(RabbitMqSettings settings, IServiceScopeFactory serviceScopeFactory, ILogger<EasyMQBusInstance> logger)
        {
            Bus = RabbitHutch.CreateBus(settings.ConnectionString, s =>
            {
                s.Register<IConsumerErrorStrategy, CustomErrorStrategy>();
            });
            _settings = settings;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;

            _eventSubscriptions = new(Bus, _logger, _serviceScopeFactory);
            _eventConsumers = new(Bus, _logger, _serviceScopeFactory);

            EventExchange = Bus.Advanced.ExchangeDeclare("Common.Application.Events.IAppEvent`1[[Core.Common.Domain.Event, Common.DomainFramework]], Common.Application",
            cfg =>
            {
            cfg.WithType("topic");
            cfg.AsDurable(true);
            });
        }

        public void InitEventSubscriptions(IImplProvider implProvider, params Assembly[] assemblies)
        {
            _eventSubscriptions.InitSubscriptions(implProvider, new EventSubscriberSeeker(), assemblies);
        }

        public void InitEventConsumers(IImplProvider implProvider, params Assembly[] assemblies)
        {
            _eventConsumers.InitSubscriptions(implProvider, new QueryHandlerSeeker(), assemblies);
        }


        internal void CancelSubscriptions()
        {
            try
            {
                Task.WaitAll(_eventSubscriptions.Subscriptions.Select(e => e.AsTask()).Concat(
                    _eventConsumers.Subscriptions.Select(e => e.AsTask())
                ).ToArray());

                foreach (var subscribption in _eventConsumers.ErrorSubscriptions.Concat(_eventSubscriptions.ErrorSubscriptions))
                {
                    subscribption.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new InfrastructureException("Exception caught while cancelling subscriptions", e);
            }

        }

        public void Dispose()
        {
            Bus.Advanced.Dispose();
            Bus.Dispose();
        }
    }
}