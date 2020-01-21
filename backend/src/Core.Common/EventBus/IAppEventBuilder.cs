using Core.Common.Command;
using Core.Common.Domain;

namespace Core.Common.EventBus
{
    public interface IAppEventBuilder
    {
        IAppEventBuilder WithCommand(CommandBase cmd);
        IAppEventBuilder WithEvent<TEvent>(TEvent @event) where TEvent : Event;
        IAppEventBuilder WithCorrelationId(CorrelationId correlationId);
        IAppEvent<TEvent> Build<TEvent>() where TEvent : Event;
    }
}