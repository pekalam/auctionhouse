using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Core.DomainFramework;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.SystemMessages;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;


namespace RabbitMq.EventBus
{
    internal interface IRabbitMqEventBus
    {
        IExchange EventExchange { get; }
        IBus Bus { get; }
    }

    internal class RabbitMqEventBus : Common.Application.Events.IEventBus, IDisposable, IRabbitMqEventBus
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqEventBus> _logger;
        private readonly IBus _bus;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly RabbitMqEventBusSubscriptions _eventSubscriptions;
        private readonly RabbitMqEventBusSubscriptions _eventConsumers;

        public IBus Bus => _bus;
        public IExchange EventExchange { get; private set; }

        public RabbitMqEventBus(RabbitMqSettings settings, ILogger<RabbitMqEventBus> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _settings = settings;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            _bus = RabbitHutch.CreateBus(_settings.ConnectionString, s =>
            {
                s.Register<IConsumerErrorStrategy, CustomErrorStrategy>();
            });

            EventExchange = _bus.Advanced.ExchangeDeclare("Common.Application.Events.IAppEvent`1[[Core.Common.Domain.Event, Common.DomainFramework]], Common.Application",
cfg =>
            {
                cfg.WithType("topic");
                cfg.AsDurable(true);
            });

            _bus.Advanced.Disconnected += (sender, args) =>
            {
                Disconnected?.Invoke(args, _logger);
            };
            _eventSubscriptions = new(_bus, _logger,_serviceScopeFactory);
            _eventConsumers = new(_bus, _logger, _serviceScopeFactory);
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


        private void RedeliverMessage(IMessage<Error> message, MessageReceivedInfo info)
        {
            try
            {
                _logger.LogWarning("Handling error message: {@message} with routingKey: {routingKey}", message, info.RoutingKey);
                //var appEvent = ParseEventFromMessage(message);
                //appEvent.RedeliveryCount++;
                //_bus.PubSub.Publish((IAppEvent<Event>)appEvent, appEvent.Event.GetType().Name);
            }
            catch (Exception)
            {
                _logger.LogError("Cannot handle error message");
            }
        }

        public void SetupErrorQueueSubscribtion()
        {
            var errorQueueName = "EasyNetQ_Default_Error_Queue";
            var queue = Bus.Advanced.QueueDeclare(errorQueueName);
            Bus.Advanced.Consume<Error>(queue, RedeliverMessage);
        }

        private void TryPublish(IAppEvent<Event> @event)
        {
            try
            {
                Bus.PubSub.Publish(@event, @event.Event.GetType().Name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not publish event");
                throw new InfrastructureException("Could not publish event", e);
            }
        }

        public void Publish<T>(IAppEvent<T> @event) where T : Event
        {
            _logger.LogDebug("Publishing event {@event}", @event);
            TryPublish(@event);
        }

        public void Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event
        {
            foreach (var @event in events)
            {
                Publish(@event);
            }
        }

        public void Dispose()
        {
            CancelSubscriptions();
            _bus.Dispose();
        }

        public event Action<EventArgs, ILogger> Disconnected = null!;
    }
}