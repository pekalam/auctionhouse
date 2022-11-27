using Core.Common.Domain;
using System.Diagnostics;

namespace Common.Application.Events
{
    public abstract class EventSubscriber<T> : IEventDispatcher where T : Event
    {
        private readonly IAppEventBuilder _eventBuilder;

        protected EventSubscriber(IAppEventBuilder eventBuilder)
        {
            _eventBuilder = eventBuilder;
        }

        async Task IEventDispatcher.Dispatch(IAppEvent<Event> msg)
        {
            using var activity = Tracing.StartActivityFromCommandContext(GetType().Name + "_" + msg.Event.EventName, msg.CommandContext);

            await HandleEvent(msg);

            activity.TraceOkStatus();
        }

        private async Task HandleEvent(IAppEvent<Event> msg)
        {
            try
            {
                var @event = _eventBuilder
                    .WithCommandContext(msg.CommandContext)
                    .WithEvent(msg.Event)
                    .WithRedeliveryCount(msg.RedeliveryCount)
                    .Build<T>();

                await Handle(@event);
            }
            catch (Exception)
            {
                Activity.Current.TraceErrorStatus("Error while handing message");
                throw;
            }
        }

        public abstract Task Handle(IAppEvent<T> appEvent);
    }

}
