using Common.Application.Commands;
using Common.Application.SagaNotifications;
using Core.Common.Domain;

namespace Common.Application.Events
{
    public class EventBusFacade
    {
        private readonly IEventBus _eventBus;
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly Func<ISagaNotifications> _sagaNotificationsFactory;

        public EventBusFacade(IEventBus eventBus, IAppEventBuilder appEventBuilder, Func<ISagaNotifications> sagaNotificationsFactory)
        {
            _eventBus = eventBus;
            _appEventBuilder = appEventBuilder;
            _sagaNotificationsFactory = sagaNotificationsFactory;
        }

        public virtual async void Publish<T>(T @event, CommandContext commandContext, ReadModelNotificationsMode consistencyMode) where T : Event //TODO task
        {
            if (consistencyMode == ReadModelNotificationsMode.Saga)
            {
                await _sagaNotificationsFactory().AddUnhandledEvent(commandContext.CorrelationId, @event);
            }
            PublishEvent(@event, commandContext, consistencyMode);
        }

        public virtual async void Publish(IEnumerable<Event> events, CommandContext commandContext, ReadModelNotificationsMode consistencyMode)//TODO task
        {
            if (consistencyMode == ReadModelNotificationsMode.Saga)
            {
                await _sagaNotificationsFactory().AddUnhandledEvents(commandContext.CorrelationId, events);
            }
            foreach (var @event in events)
            {
                PublishEvent(@event, commandContext, consistencyMode);
            }
        }

        private void PublishEvent<T>(T @event, CommandContext commandContext, ReadModelNotificationsMode consistencyMode) where T : Event
        {
            var appEvent = _appEventBuilder
                .WithReadModelNotificationsMode(consistencyMode)
                .WithCommandContext(commandContext)
                .WithEvent(@event)
                .Build<T>();
            _eventBus.Publish(appEvent);
        }

    }
}
