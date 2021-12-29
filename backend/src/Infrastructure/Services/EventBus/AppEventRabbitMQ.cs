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

        public AppEventRabbitMQ(T @event, CommandContext commandContext)
        {
            Event = @event;
            CommandContext = commandContext;
        }
    }
}