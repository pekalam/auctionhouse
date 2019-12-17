using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Core.Command;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;
using EasyNetQ;
using EasyNetQ.NonGeneric;
using EasyNetQ.SystemMessages;
using EasyNetQ.Topology;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IEventBus = Core.Common.EventBus.IEventBus;

[assembly: InternalsVisibleTo("FunctionalTests")]
[assembly: InternalsVisibleTo("IntegrationTests")]
namespace Infrastructure.Services.EventBus
{
    internal class RabbitMqEventConsumerFactory
    {
        public Func<IEventConsumer> FactoryFunc { get; }
        public string MessageName { get; }

        public RabbitMqEventConsumerFactory(Func<IEventConsumer> factoryFunc, string messageName)
        {
            FactoryFunc = factoryFunc;
            MessageName = messageName;
        }
    }

    public class RabbitMqEventBus : IEventBus
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqEventBus> _logger;
        private IBus _bus;
        private List<ISubscriptionResult> _subscriptions = new List<ISubscriptionResult>();
        private List<ISubscriptionResult> _cmdSubscriptions = new List<ISubscriptionResult>();


        public RabbitMqEventBus(RabbitMqSettings settings, ILogger<RabbitMqEventBus> logger)
        {
            _settings = settings;
            _logger = logger;
            _bus = RabbitHutch.CreateBus(_settings.ConnectionString);
        }

        internal void InitSubscribers(params RabbitMqEventConsumerFactory[] subscriberFactories)
        {

            SetupErrorQueueSubscribtion();

            foreach (var eventConsumer in subscriberFactories)
            {
                Subscribe(eventConsumer);
            }
        }

        internal void InitSubscribers(string assemblyName, IImplProvider implProvider)
        {
            SetupErrorQueueSubscribtion();

            var eventConsumerTypes = Assembly.Load(assemblyName)
                .GetTypes()
                .Where(type => type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(EventConsumer<>))
                .ToArray();

            foreach (var consumerType in eventConsumerTypes)
            {
                var evType = consumerType.BaseType.GetGenericArguments()[0];
                var factory = new RabbitMqEventConsumerFactory(() => (IEventConsumer) implProvider.Get(consumerType), evType.Name);
                Subscribe(factory);
            }
        }

        internal void InitCommandSubscribers(string assemblyName, IImplProvider implProvider, IMediator mediator)
        {
            var commandHandlerTypes = Assembly.Load(assemblyName)
                .GetTypes()
                .Where(type =>
                    type.BaseType != null && type.BaseType.IsGenericType &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(CommandHandlerBase<>))
                .ToArray();

            foreach (var handlerType in commandHandlerTypes)
            {
                var cmdType = handlerType.BaseType.GetGenericArguments()[0];
                CommandSubscribe(cmdType, handlerType, implProvider, mediator);
            }
        }

        private void CommandSubscribe(Type cmdType, Type handlerType, IImplProvider implProvider, IMediator mediator)
        {
            var cmdName = cmdType.Name;
            var subscription = _bus.Subscribe(typeof(ICommand), cmdName,
                cmd =>
                {
                    var handler = implProvider.Get<QueuedCommandHandler>();
                    handler.Handle(cmd as ICommand, handlerType);
                },
                configuration =>
                {
                    configuration.WithTopic(cmdName);
                    configuration.WithQueueName(cmdName);
                });
            _cmdSubscriptions.Add(subscription);
        }

        internal void CancelSubscriptions()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.ConsumerCancellation.Dispose();
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
            try
            {
                _logger.LogError($"Error message {info.RoutingKey}");
                IAppEvent<Event> commandEvent = ParseEventFromMessage(message);
                if (commandEvent != null)
                {
                    var commandName = commandEvent.Command?.GetType()
                        .Name;
                    var handler = RollbackHandlerRegistry.GetCommandRollbackHandler(commandName);
                    handler.Rollback(commandEvent);
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

        private void Subscribe(RabbitMqEventConsumerFactory eventConsumerFactory)
        {
            string toCamel(string s) => Char.ToLower(s[0]) + s.Substring(1);
            string messageName = toCamel(eventConsumerFactory.MessageName);
            Func<IEventConsumer> factory = eventConsumerFactory.FactoryFunc;

            var subscription = _bus.Subscribe(typeof(IAppEvent<Event>), messageName,
                o => factory
                        .Invoke()
                        .Dispatch(o),
                configuration =>
                {
                    configuration.WithTopic(messageName);
                    configuration.WithQueueName(messageName);
                });
            _subscriptions.Add(subscription);
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

        public void Send<T>(T command) where T : ICommand
        {
            _bus.Publish(typeof(ICommand), command, command.GetType().Name);
        }
    }
}