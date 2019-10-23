using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Common.ApplicationServices;
using Core.Common.Domain;
using Core.Common.EventBus;
using EasyNetQ;
using EasyNetQ.NonGeneric;
using EasyNetQ.SystemMessages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IEventBus = Core.Common.EventBus.IEventBus;

[assembly: InternalsVisibleTo("FunctionalTests")]
namespace Infrastructure.Services.EventBus
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqEventBus> _logger;
        private IBus _bus;


        public RabbitMqEventBus(RabbitMqSettings settings, ILogger<RabbitMqEventBus> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        internal void Init(params IEventConsumer[] subscribers)
        {
            _bus = RabbitHutch.CreateBus(_settings.ConnectionString);

            SetupErrorQueueSubscribtion();

            foreach (var eventConsumer in subscribers)
            {
                Subscribe(eventConsumer);
            }
        }

        private IAppEvent<Event> ParseEventFromMessage(IMessage<Error> message)
        {
            try
            {
                return JsonConvert.DeserializeObject<AppEventRabbitMQ<Event>>(message.Body.Message, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Cannot parse event from error message {e.ToString()}");
                return null;
            }
        }

        private void HandleErrorMessage(IMessage<Error> message, MessageReceivedInfo info)
        {
            _logger.LogError($"Error message {info.RoutingKey}");
            var commandEvent = ParseEventFromMessage(message);
            if (commandEvent != null)
            {
                var commandName = commandEvent.Command.GetType().Name;
                var handler = RollbackHandlerRegistry.GetCommandRollbackHandler(commandName);
                handler.Rollback(commandEvent);
            }
        }

        private void SetupErrorQueueSubscribtion()
        {
            var errorQueueName = "EasyNetQ_Default_Error_Queue";
            var queue = _bus.Advanced.QueueDeclare(errorQueueName);

            _bus.Advanced.Consume<Error>(queue, HandleErrorMessage);
        }

        private void Subscribe(IEventConsumer eventConsumer)
        {
            string toCamel(string i) => (Char.ToLower(i[0])) + i.Substring(1);

            _bus.Subscribe(typeof(IAppEvent<Event>), eventConsumer.MessageType.Name, o => eventConsumer.Dispatch(o), configuration =>
            {
                configuration.WithTopic(toCamel(eventConsumer.MessageType.Name));
                configuration.WithQueueName(toCamel(eventConsumer.MessageType.Name));
            });
        }

        public void Publish<T>(IAppEvent<T> @event) where T : Event
        {
            _bus.Publish(@event, @event.Event.EventName);
            
           //_bus.Advanced.Publish(new Exchange("Core.Interfaces.EventBus.IAppEvent`1[[Core.DomainModel.Event, Core]], Core"), @event.Event.EventName, true, new Message<IAppEvent<T>>(@event));
        }

        public void Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event
        {
            foreach (var @event in events)
            {
                Publish(@event);
            }
        }
    }
}
