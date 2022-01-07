using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using EasyNetQ;
using EasyNetQ.Internals;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace RabbitMq.EventBus
{
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
            _logger.LogDebug("Initializing subscriptions in " + string.Join(' ', assemblies.Select(a => a.FullName)));
            var handlerToEventType = handlerSeeker.Seek(assemblies).ToArray();
            _logger.LogDebug("Found {Count} event handlers", handlerToEventType.Length);


            foreach (var (handlerType, eventType) in handlerToEventType)
            {
                _logger.LogInformation(handlerType.Name + "is subscribing to " + eventType.Name);
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
}