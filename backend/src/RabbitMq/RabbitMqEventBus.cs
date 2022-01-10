using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Core.DomainFramework;
using EasyNetQ;
using EasyNetQ.SystemMessages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;


namespace RabbitMq.EventBus
{

    internal class RabbitMqEventBus : Common.Application.Events.IEventBus
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqEventBus> _logger;
        private readonly IBus _bus;

        private readonly RabbitMqEventBusSubscriptions _eventSubscriptions;
        private readonly RabbitMqEventBusSubscriptions _eventConsumers;

        public RabbitMqEventBus(RabbitMqSettings settings, ILogger<RabbitMqEventBus> logger)
        {
            _settings = settings;
            _logger = logger;
            _bus = RabbitHutch.CreateBus(_settings.ConnectionString);
            _bus.Advanced.Disconnected += (sender, args) =>
            {
                Disconnected?.Invoke(args, _logger);
            };
            _eventSubscriptions = new(_bus, _logger);
            _eventConsumers = new(_bus, _logger);
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
            }
            catch (Exception e)
            {
                throw new InfrastructureException("Exception caught while cancelling subscriptions", e);
            }

        }

        private IAppEvent<Event> ParseEventFromMessage(IMessage<Error> message)
        {
            try
            {
                return JsonConvert.DeserializeObject<AppEventRabbitMQ<Event>>(message.Body.Message,
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                        DateParseHandling = DateParseHandling.DateTime
                    }) ?? throw new NullReferenceException("DeserializeObject returned null value");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot parse event from error message: {message}", message);
                throw new InfrastructureException("MQ message parse error", e);
            }
        }

        private void HandleErrorMessage(IMessage<Error> message, MessageReceivedInfo info)
        {
            try
            {
                _logger.LogWarning("Handling error message: {@message} with routingKey: {routingKey}", message, info.RoutingKey);
                IAppEvent<Event> commandEvent = ParseEventFromMessage(message);
                if (commandEvent != null)
                {
                    var commandName = commandEvent.CommandContext.Name;
                }
                else
                {
                    _logger.LogError("Parsed command event is null! RoutingKey: {routingKey}", info.RoutingKey);
                }
            }
            catch (Exception)
            {
                _logger.LogError("Cannot handle error message");
            }
        }

        private void SetupErrorQueueSubscribtion()
        {
            var errorQueueName = "EasyNetQ_Default_Error_Queue";
            var queue = _bus.Advanced.QueueDeclare(errorQueueName);
            _bus.Advanced.Consume<Error>(queue, HandleErrorMessage);
        }

        private void TryPublish(IAppEvent<Event> @event)
        {
            try
            {
                _bus.PubSub.Publish(@event, @event.Event.GetType().Name);
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

        public event Action<EventArgs, ILogger> Disconnected = null!;
    }
}