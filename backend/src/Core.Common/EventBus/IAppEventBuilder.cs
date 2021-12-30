using Core.Common.Command;
using Core.Common.Domain;

namespace Core.Common.EventBus
{
    public interface IAppEventBuilder
    {
        IAppEventBuilder WithReadModelNotificationsMode(ReadModelNotificationsMode consistencyMode);
        IAppEventBuilder WithCommandContext(CommandContext commandContext);
        IAppEventBuilder WithEvent<TEvent>(TEvent @event) where TEvent : Event;
        IAppEvent<TEvent> Build<TEvent>() where TEvent : Event;
    }
}