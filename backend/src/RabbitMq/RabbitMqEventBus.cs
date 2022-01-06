using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using EasyNetQ;
using EasyNetQ.Internals;
using EasyNetQ.SystemMessages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;


namespace RabbitMq.EventBus
{
    internal class QueryHandlerSeeker : IHandlerSeeker
    {
        public IEnumerable<(Type handlerType, Type eventType)> Seek(params Assembly[] assemblies)
        {
            return assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(EventConsumer<,>))
                .Select(t =>
                {
                    return (t, t.BaseType.GenericTypeArguments[0]);
                });
        }
    }

    internal interface IHandlerSeeker
    {
        IEnumerable<(Type handlerType, Type eventType)> Seek(params Assembly[] assemblies);
    }

    internal class EventSubscriberSeeker : IHandlerSeeker
    {
        public IEnumerable<(Type handlerType, Type eventType)> Seek(params Assembly[] assemblies)
        {
            return assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(EventSubscriber<>))
                .Select(t =>
                {
                    return (t, t.BaseType.GenericTypeArguments[0]);
                });
        }
    }


    internal class RabbitMqEventBusSubscriptions
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;
        private readonly List<AwaitableDisposable<ISubscriptionResult>> _subscriptions = new();

        public RabbitMqEventBusSubscriptions(IBus bus, ILogger logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public IReadOnlyList<AwaitableDisposable<ISubscriptionResult>> Subscriptions => _subscriptions;

        public void InitSubscriptions(IImplProvider implProvider, IHandlerSeeker handlerSeeker, params Assembly[] assemblies)
        {
            var handlerToEventType = handlerSeeker.Seek(assemblies).ToArray();
            _logger.LogDebug("Found {Count} event handlers", handlerToEventType.Length);


            foreach (var (handlerType, eventType) in handlerToEventType)
            {
                var sub = _bus.PubSub.SubscribeAsync<IAppEvent<Event>>(eventType.Name,
                    (appEvent, ct) =>
                    {
                        var dispatcher = (IEventDispatcher)implProvider.Get(handlerType);
                        return dispatcher.Dispatch(appEvent);
                    }, conf =>
                    {
                        conf.WithTopic(eventType.Name);
                        conf.WithQueueName(handlerType.Name);
                    });
                _subscriptions.Add(sub);
            }
        }
    }

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
            Task.WaitAll(_eventSubscriptions.Subscriptions.Select(e => e.AsTask()).Concat(
                    _eventConsumers.Subscriptions.Select(e => e.AsTask())
                ).ToArray());
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
                    });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot parse event from error message: {message}", message);
                return null;
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


        public void Publish<T>(IAppEvent<T> @event) where T : Event
        {
            _logger.LogDebug("Publishing event {@event}", @event);
            _bus.PubSub.Publish(@event, @event.Event.GetType().Name);
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