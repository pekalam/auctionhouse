using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using EasyNetQ;
using EasyNetQ.NonGeneric;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.EventBus
{
    public class RabbitMqQueuedCommandBus : IQueuedCommandBus
    {
        private List<ISubscriptionResult> _cmdSubscriptions = new List<ISubscriptionResult>();
        private readonly ILogger<RabbitMqQueuedCommandBus> _logger;
        private IBus _bus;

        public event Action<EventArgs, ILogger> Disconnected;

        public RabbitMqQueuedCommandBus(RabbitMqSettings settings, ILogger<RabbitMqQueuedCommandBus> logger)
        {
            _logger = logger;

            _bus = RabbitHutch.CreateBus(settings.ConnectionString); //TODO too mant instacnes?
            _bus.Advanced.Disconnected += (sender, args) =>
            {
                Disconnected?.Invoke(args, _logger);
            };
        }


        internal void InitQueuedCommandSubscribers(string assemblyName, IImplProvider implProvider)
        {
            var commandHandlerTypes = Assembly.Load(assemblyName)
                .GetTypes()
                .Where(type =>
                    type.BaseType != null && type.BaseType.IsGenericType &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(CommandHandlerBase<>)) //TODO choose by attribute presence
                .ToArray();
            _logger.LogDebug("Found commandHandlers {list} in assembly: {assemblyName}", commandHandlerTypes.Select(f => f.Name).ToArray(), assemblyName);

            foreach (var handlerType in commandHandlerTypes)
            {
                var cmdType = handlerType.BaseType.GetGenericArguments()[0];
                Subscribe(cmdType, implProvider);
            }
        }

        private void Subscribe(Type cmdType, IImplProvider implProvider)
        {
            var cmdName = cmdType.Name;
            var queueName = "Queued_" + cmdName;
            var subscription = _bus.Subscribe(typeof(QueuedCommand), cmdName,
                obj =>
                {
                    var cmd = (QueuedCommand)obj;
                    var cmdContext = ((ICommandContextOwner)cmd.AppCommand).CommandContext;
                    _logger.LogDebug("Reveived command message from MQ: {@command}", cmd);
                    if (cmdContext.WSQueued)
                    {
                        var handler = implProvider.Get<WSQueuedCommandHandler>();
                        handler.Handle(cmd);
                    }
                    else if (cmdContext.HttpQueued)
                    {
                        var handler = implProvider.Get<HTTPQueuedCommandHandler>();
                        handler.Handle(cmd);
                    }
                },
                configuration =>
                {
                    configuration.WithTopic(cmdName);
                    configuration.WithQueueName(queueName);
                });
            _cmdSubscriptions.Add(subscription);
        }

        internal void CancelCommandSubscriptions()
        {
            foreach (var subscription in _cmdSubscriptions)
            {
                subscription.Dispose();
            }
            _cmdSubscriptions.Clear();
        }

        public void Publish<TCmd>(QueuedCommand command)
        {
            _logger.LogDebug("Publishing queued command {@command}", command);
            _bus.Publish(command, typeof(TCmd).Name);
        }
    }
}