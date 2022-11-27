using Common.Application;
using Common.Application.Commands;
using Common.Application.Mediator;
using Core.Command.Handler;
using EasyNetQ;
using EasyNetQ.Internals;
using Microsoft.Extensions.Logging;

namespace RabbitMq.EventBus
{
    public class RabbitMqQueuedCommandBus : IQueuedCommandBus
    {
        private readonly Dictionary<Type, AwaitableDisposable<ISubscriptionResult>> _cmdSubscriptions = new();
        private readonly ILogger<RabbitMqQueuedCommandBus> _logger;
        private readonly IBus _bus;

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

        public void PreparePublish(IImplProvider implProvider, ICommand command)
        {
            var cmdType = command.GetType();
            if (!_cmdSubscriptions.ContainsKey(cmdType))
            {
                Subscribe(cmdType, implProvider);
            }
        }

        private void Subscribe(Type cmdType, IImplProvider implProvider)
        {
            var cmdName = cmdType.Name;
            var queueName = "Queued_" + cmdName;
            var subscription = _bus.PubSub.SubscribeAsync<QueuedCommand>(cmdName,
                cmd =>
                {
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
            _cmdSubscriptions.Add(cmdType, subscription);
        }

        internal void CancelCommandSubscriptions()
        {
            Task.WaitAll(_cmdSubscriptions.Values.Select(t => t.AsTask()).ToArray());
            _cmdSubscriptions.Clear();
        }

        public void Publish<TCmd>(QueuedCommand command)
        {
            _logger.LogDebug("Publishing queued command {@command}", command);
            _bus.PubSub.Publish(command, typeof(TCmd).Name);
        }
    }
}