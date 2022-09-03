using Common.Application.Commands;
using Core.Common.Domain;

namespace Common.Application.Events
{
    public interface IAppEventBuilder
    {
        IAppEventBuilder WithCommandContext(CommandContext commandContext);
        IAppEventBuilder WithEvent<TEvent>(TEvent @event) where TEvent : Event;
        IAppEventBuilder WithRedeliveryCount(int redeliveryCount);
        IAppEvent<TEvent> Build<TEvent>() where TEvent : Event;
    }
}