using Core.Command;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;

namespace Infrastructure.Services.EventBus
{
    public class AppEventRabbitMQ<T> : IAppEvent<T> where T : Event
    {
        public CommandBase CommandBase { get; }
        public CorrelationId CorrelationId { get; }
        public T Event { get; }

        public AppEventRabbitMQ(CorrelationId correlationId, T @event, CommandBase commandBase)
        {
            CorrelationId = correlationId;
            Event = @event;
            CommandBase = commandBase;
        }
    }
}