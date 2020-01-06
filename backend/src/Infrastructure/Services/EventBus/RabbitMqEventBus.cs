using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Core.Command;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;
using Core.Query.EventHandlers;
using EasyNetQ;
using EasyNetQ.NonGeneric;
using EasyNetQ.SystemMessages;
using EasyNetQ.Topology;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IEventBus = Core.Common.EventBus.IEventBus;

[assembly: InternalsVisibleTo("Test.FunctionalTests")]
[assembly: InternalsVisibleTo("Test.IntegrationTests")]

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
            _bus.Advanced.Disconnected += (sender, args) =>
            {
                Disconnected(args, _logger);
            };
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
                .Where(type =>
                    type.BaseType != null && type.BaseType.IsGenericType &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(EventConsumer<>))
                .ToArray();
            _logger.LogDebug("Found EventConsumers {list} in assembly: {assemblyName}", eventConsumerTypes.Select(f => f.Name).ToArray(), assemblyName);

            foreach (var consumerType in eventConsumerTypes)
            {
                var evType = consumerType.BaseType.GetGenericArguments()[0];
                var factory = new RabbitMqEventConsumerFactory(() => (IEventConsumer) implProvider.Get(consumerType),
                    evType.Name);
                Subscribe(factory);
            }
        }

        internal void InitCommandSubscribers(string assemblyName, IImplProvider implProvider)
        {
            var commandHandlerTypes = Assembly.Load(assemblyName)
                .GetTypes()
                .Where(type =>
                    type.BaseType != null && type.BaseType.IsGenericType &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(CommandHandlerBase<>))
                .ToArray();
            _logger.LogDebug("Found commandHandlers {list} in assembly: {assemblyName}", commandHandlerTypes.Select(f => f.Name).ToArray(), assemblyName);

            foreach (var handlerType in commandHandlerTypes)
            {
                var cmdType = handlerType.BaseType.GetGenericArguments()[0];
                CommandSubscribe(cmdType, implProvider);
            }
        }

        //tests
        internal void CancelCommandSubscriptions()
        {
            foreach (var subscription in _cmdSubscriptions)
            {
                subscription.Dispose();
            }
            _cmdSubscriptions.Clear();
        }

        private void CommandSubscribe(Type cmdType, IImplProvider implProvider)
        {
            var cmdName = cmdType.Name;
            var subscription = _bus.Subscribe(typeof(ICommand), cmdName,
                obj =>
                {
                    var cmd = obj as ICommand;
                    _logger.LogDebug("Reveived command message from MQ: {@command}", cmd);
                    if (cmd.WSQueued)
                    {
                        var handler = implProvider.Get<WSQueuedCommandHandler>();
                        handler.Handle(cmd);
                    }
                    else if (cmd.HttpQueued)
                    {
                        var handler = implProvider.Get<HTTPQueuedCommandHandler>();
                        handler.Handle(cmd);
                    }
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
                _logger.LogError(e, "Cannot parse event from error message: {message}", message);
                return null;
            }
        }

        private void TryExecuteCommandRollback(ICommandRollbackHandler handler, IAppEvent<Event> commandEvent)
        {
            try
            {
                handler.Rollback(commandEvent);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Catched exception in command rollback handler. Command: {@command}", commandEvent);
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
                    var commandName = commandEvent.Command?.GetType()
                        .Name;
                    var handler = RollbackHandlerRegistry.GetCommandRollbackHandler(commandName);
                    TryExecuteCommandRollback(handler, commandEvent);
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

        private void Subscribe(RabbitMqEventConsumerFactory eventConsumerFactory)
        {
            string toCamel(string s) => Char.ToLower(s[0]) + s.Substring(1);
            string messageName = toCamel(eventConsumerFactory.MessageName);
            Func<IEventConsumer> factory = eventConsumerFactory.FactoryFunc;

            _logger.LogDebug("Initializing subscriber: {s}", messageName);

            var subscription = _bus.Subscribe(typeof(IAppEvent<Event>), messageName,
                o =>
                {
                    _logger.LogDebug("Received message from MQ: {name}", messageName);
                    factory
                        .Invoke()
                        .Dispatch(o);
                },
                configuration =>
                {
                    configuration.WithTopic(messageName);
                    configuration.WithQueueName(messageName);
                });
            _subscriptions.Add(subscription);
        }

        public void Publish<T>(IAppEvent<T> @event) where T : Event
        {
            _logger.LogDebug("Publishing event {@event}", @event);
            _bus.Publish(@event, @event.Event.EventName);
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
            _logger.LogDebug("Publishing command {@command}", command);
            _bus.Publish(typeof(ICommand), command, command.GetType().Name);
        }

        public event Action<EventArgs, ILogger> Disconnected;
    }
}