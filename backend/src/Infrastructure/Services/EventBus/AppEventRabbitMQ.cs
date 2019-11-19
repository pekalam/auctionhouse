using Core.Command;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;

namespace Infrastructure.Services.EventBus
{
    public class AppEventRabbitMQ<T> : IAppEvent<T> where T : Event
    {
        public ICommand Command { get; }
        public CorrelationId CorrelationId { get; }
        public T Event { get; }

        public AppEventRabbitMQ(CorrelationId correlationId, T @event, ICommand command)
        {
            CorrelationId = correlationId;
            Event = @event;
            Command = command;
        }
    }
}