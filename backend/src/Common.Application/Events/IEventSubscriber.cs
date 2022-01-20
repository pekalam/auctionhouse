using Core.Common.Domain;

namespace Common.Application.Events
{
    public interface IEventDispatcher
    {
        Task Dispatch(IAppEvent<Event> msg);
    }

    public abstract class EventSubscriber<T> : IEventDispatcher where T : Event
    {
        private readonly IAppEventBuilder _eventBuilder;

        protected EventSubscriber(IAppEventBuilder eventBuilder)
        {
            _eventBuilder = eventBuilder;
        }

        Task IEventDispatcher.Dispatch(IAppEvent<Event> msg)
        {
            var @event = _eventBuilder
                .WithCommandContext(msg.CommandContext)
                .WithEvent(msg.Event)
                .WithRedeliveryCount(msg.RedeliveryCount)
                .Build<T>();

            return Handle(@event);
        }

        public abstract Task Handle(IAppEvent<T> appEvent);
    }

}
