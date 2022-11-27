using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Core.DomainFramework;
using EasyNetQ;
using EasyNetQ.SystemMessages;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RabbitMq.EventBus
{
    internal class RabbitMqEventBus : Common.Application.Events.IEventBus
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqEventBus> _logger;
        private readonly IEasyMQBusInstance _rabbitMq;

        public RabbitMqEventBus(RabbitMqSettings settings, ILogger<RabbitMqEventBus> logger, IEasyMQBusInstance rabbitmq)
        {
            settings.ValidateSettings();
            _settings = settings;
            _logger = logger;
            _rabbitMq = rabbitmq;
        }

        private void HandleErrorMessage(IMessage<Error> message, MessageReceivedInfo info)
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
            var queue = _rabbitMq.Bus.Advanced.QueueDeclare(errorQueueName);
            _rabbitMq.Bus.Advanced.Consume<Error>(queue, HandleErrorMessage);
        }

        private async Task PublishInternal(IAppEvent<Event> @event)
        {
            try
            {
                Tracing.SetActivityContextData(@event.CommandContext);
                await _rabbitMq.Bus.PubSub.PublishAsync(@event, @event.Event.GetType().Name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not publish event");
                throw new InfrastructureException("Could not publish event", e);
            }
        }

        public async Task Publish<T>(IAppEvent<T> @event) where T : Event
        {
            _logger.LogDebug("Publishing event {@event}", @event);
            await PublishInternal(@event);
        }

        public async Task Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event
        {
            foreach (var @event in events)
            {
                await Publish(@event);
            }
        }
    }
}