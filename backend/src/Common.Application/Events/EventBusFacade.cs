﻿using Common.Application.Commands;
using Common.Application.SagaNotifications;
using Core.Common.Domain;

namespace Common.Application.Events
{
    public class EventBusFacade
    {
        private readonly IEventBus _eventBus;
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly Lazy<ISagaNotifications> _sagaNotificationsFactory;

        public EventBusFacade(IEventBus eventBus, IAppEventBuilder appEventBuilder, Lazy<ISagaNotifications> sagaNotificationsFactory)
        {
            _eventBus = eventBus;
            _appEventBuilder = appEventBuilder;
            _sagaNotificationsFactory = sagaNotificationsFactory;
        }

        public virtual void Publish<T>(T @event, CommandContext commandContext, ReadModelNotificationsMode consistencyMode) where T : Event //TODO task
        {
            if (consistencyMode == ReadModelNotificationsMode.Saga)
            {
                _sagaNotificationsFactory.Value.AddUnhandledEvent(commandContext.CorrelationId, @event).GetAwaiter().GetResult();
            }
            PublishEvent(@event, commandContext, consistencyMode);
        }

        public virtual void Publish(IEnumerable<Event> events, CommandContext commandContext, ReadModelNotificationsMode consistencyMode)//TODO task
        {
            if (consistencyMode == ReadModelNotificationsMode.Saga)
            {
                _sagaNotificationsFactory.Value.AddUnhandledEvents(commandContext.CorrelationId, events).GetAwaiter().GetResult();
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
