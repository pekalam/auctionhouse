using Common.Application.Commands;
using Core.Common.Domain;

namespace Common.Application.Events
{
    public interface IAppEventBuilder
    {
        IAppEventBuilder WithReadModelNotificationsMode(ReadModelNotificationsMode consistencyMode);
        IAppEventBuilder WithCommandContext(CommandContext commandContext);
        IAppEventBuilder WithEvent<TEvent>(TEvent @event) where TEvent : Event;
        IAppEvent<TEvent> Build<TEvent>() where TEvent : Event;
    }
}