using Core.Command;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;

namespace Infrastructure.Services.EventBus
{
    public class AppEventRabbitMQBuilder : IAppEventBuilder
    {
        private Event _event;
        private CommandContext _commandContext;

        public IAppEventBuilder WithCommandContext(CommandContext commandContext)
        {
            _commandContext = commandContext;
            return this;
        }

        public IAppEventBuilder WithEvent<TEvent>(TEvent @event) where TEvent : Event
        {
            _event = @event;
            return this;
        }

        public IAppEvent<TEvent> Build<TEvent>() where TEvent : Event
        {
            return new AppEventRabbitMQ<TEvent>((TEvent)_event, _commandContext);
        }
    }
}