using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;

namespace RabbitMq.EventBus
{
    public class AppEventRabbitMQ<T> : IAppEvent<T> where T : Event
    {
        public CommandContext CommandContext { get; }
        public T Event { get; }
        public ReadModelNotificationsMode ReadModelNotifications { get; }

        public AppEventRabbitMQ(CommandContext commandContext, T @event, ReadModelNotificationsMode consistencyMode)
        {
            CommandContext = commandContext;
            Event = @event;
            ReadModelNotifications = consistencyMode;
        }
    }
}