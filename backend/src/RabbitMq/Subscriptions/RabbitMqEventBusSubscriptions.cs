using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Core.DomainFramework;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Internals;
using EasyNetQ.SystemMessages;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;

namespace RabbitMq.EventBus
{
    internal class CustomErrorStrategy : DefaultConsumerErrorStrategy
    {
        public CustomErrorStrategy(IPersistentConnection connection, ISerializer serializer, IConventions conventions, ITypeNameSerializer typeNameSerializer, IErrorMessageSerializer errorMessageSerializer, ConnectionConfiguration configuration) : base(connection, serializer, conventions, typeNameSerializer, errorMessageSerializer, configuration)
        {
        }

        public override AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
            string newRoutingKey;
            if (!context.ReceivedInfo.RoutingKey.Contains("_Error_"))
            {
                newRoutingKey = context.ReceivedInfo.RoutingKey + "_Error_" + context.ReceivedInfo.Queue;

            }
            else
            {
                newRoutingKey = context.ReceivedInfo.RoutingKey;
            }
            var receivedInfo = new MessageReceivedInfo(context.ReceivedInfo.ConsumerTag,
                context.ReceivedInfo.DeliveryTag,
                context.ReceivedInfo.Redelivered,
                context.ReceivedInfo.Exchange,
                newRoutingKey,
                context.ReceivedInfo.Queue); ;

            return base.HandleConsumerError(new ConsumerExecutionContext(context.Handler, receivedInfo, context.Properties, context.Body), exception);
        }
    }

    internal class RabbitMqEventBusSubscriptions
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;
        private readonly List<AwaitableDisposable<ISubscriptionResult>> _subscriptions = new();
        private readonly List<IDisposable> _errorSubscriptions = new();
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMqEventBusSubscriptions(IBus bus, ILogger logger, IServiceScopeFactory serviceScopeFactory)
        {
            _bus = bus;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IReadOnlyList<AwaitableDisposable<ISubscriptionResult>> Subscriptions => _subscriptions;
        public IReadOnlyList<IDisposable> ErrorSubscriptions => _errorSubscriptions;

        public void InitSubscriptions(IImplProvider implProvider, IHandlerSeeker handlerSeeker, params Assembly[] assemblies)
        {
            _logger.LogDebug("Initializing subscriptions in " + string.Join(' ', assemblies.Select(a => a.FullName)));
            var handlerToEventType = handlerSeeker.Seek(assemblies).ToArray();
            _logger.LogDebug("Found {Count} event handlers", handlerToEventType.Length);

            foreach (var (handlerType, eventType) in handlerToEventType)
            {
                var subscriptionId = string.Join('_', handlerType.AssemblyQualifiedName!.Split(',')[0]) + eventType.Name;
                _logger.LogInformation($"{handlerType.Name} with subscriptionId= {subscriptionId} is subscribing to {eventType.Name}");

                var errorRoutingKey = eventType.Name + "_Error_" + subscriptionId;

                SetupSubscription(implProvider, handlerType, eventType, subscriptionId, errorRoutingKey);
                SetupErrorSubscribption(eventType, subscriptionId, errorRoutingKey);
            }
        }

        private void SetupSubscription(IImplProvider implProvider, Type handlerType, Type eventType, string subscriptionId, string errorRoutingKey)
        {
            var sub = _bus.PubSub.SubscribeAsync<IAppEvent<Event>>(subscriptionId,
                async (appEvent, ct) =>
                {
                    _logger.LogDebug("Handling event {@eventType} by {@handlerType}", eventType.Name, handlerType.Name);
                    using (var scope = implProvider.Get<IServiceScopeFactory>().CreateScope())
                    {
                        var dispatcher = (IEventDispatcher)scope.ServiceProvider.GetRequiredService(handlerType);
                        _logger.LogDebug("Handling event by {@handlerType}", handlerType.Name);
                        try
                        {
                            await dispatcher.Dispatch(appEvent);
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, "Event handler {@handlerType} thrown an exception", handlerType.Name);
                            throw;
                        }
                    }
                }, conf =>
                {
                    conf.WithQueueName(subscriptionId);
                    conf.WithTopic(eventType.Name);
                    conf.WithTopic(errorRoutingKey);

                });
            _subscriptions.Add(sub);
        }

        private void SetupErrorSubscribption(Type eventType, string subscriptionId, string errorRoutingKey)
        {
            var errorQueueName = "Error_" + eventType.Name;
            var errorQ = _bus.Advanced.QueueDeclare(errorQueueName);
            var errorEx = _bus.Advanced.ExchangeDeclare("ErrorExchange_" + eventType.Name + "_Error_" + subscriptionId, "direct", true);
            _bus.Advanced.Bind(errorEx, errorQ, errorRoutingKey);
            var errorSubscription = _bus.Advanced.Consume(errorQ, (Action<IMessage<Error>, MessageReceivedInfo>)((err, info) =>
            {
                _logger.LogInformation("Received error message in subscription {@subscriptionId} with routing key {@routingKey}", subscriptionId, errorRoutingKey);

                using var scope = _serviceScopeFactory.CreateScope();
                var errorEventOutboxStore = scope.ServiceProvider.GetRequiredService<IErrorEventOutboxStore>();

                SaveErrorOutboxItem(err, info, errorEventOutboxStore);
            }));
            _errorSubscriptions.Add(errorSubscription);
        }

        private void SaveErrorOutboxItem(IMessage<Error> err, MessageReceivedInfo info, IErrorEventOutboxStore errorEventOutboxStore)
        {
            try
            {
                errorEventOutboxStore.Save(new ErrorEventOutboxItem
                {
                    Timestamp = ErrorEventOutboxItemTimestampFactory.CreateTimestamp(),
                    MessageProperties = err.Body.BasicProperties,
                    MessageJson = err.Body.Message,
                    RoutingKey = info.RoutingKey
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception caught while saving item in {nameof(IErrorEventOutboxStore)}");
                throw;
            }
        }
    }
}