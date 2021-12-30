using Core.Command;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;

namespace Infrastructure.Services.EventBus
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