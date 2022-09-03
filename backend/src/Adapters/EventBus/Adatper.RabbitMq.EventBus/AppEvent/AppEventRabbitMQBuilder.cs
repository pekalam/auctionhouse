using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;

namespace RabbitMq.EventBus
{
    public class AppEventRabbitMQBuilder : IAppEventBuilder
    {
        private Event _event;
        private CommandContext _commandContext;
        private int _redeliveryCount;

        public IAppEventBuilder WithCommandContext(CommandContext commandContext)
        {
            _commandContext = commandContext;
            return this;
        }

        public IAppEventBuilder WithEvent<TEvent>(TEvent @event) where TEvent : Event //TODO unnecessary type param
        {
            _event = @event;
            return this;
        }

        public IAppEvent<TEvent> Build<TEvent>() where TEvent : Event
        {
            return new AppEventRabbitMQ<TEvent>(_commandContext, (TEvent)_event, _redeliveryCount);
        }

        public IAppEventBuilder WithRedeliveryCount(int redeliveryCount)
        {
            _redeliveryCount = redeliveryCount;
            return this;
        }
    }
}