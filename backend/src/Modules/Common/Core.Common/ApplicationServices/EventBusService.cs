using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;

namespace Core.Common.ApplicationServices
{
    public class EventBusService
    {
        private readonly IEventBus _eventBus;
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly Func<ISagaNotifications> _sagaNotificationsFactory;

        public EventBusService(IEventBus eventBus, IAppEventBuilder appEventBuilder, Func<ISagaNotifications> sagaNotificationsFactory)
        {
            _eventBus = eventBus;
            _appEventBuilder = appEventBuilder;
            _sagaNotificationsFactory = sagaNotificationsFactory;
        }

        public virtual async void Publish<T>(T @event, CommandContext commandContext, ReadModelNotificationsMode consistencyMode) where T : Event //TODO task
        {
            if(consistencyMode == ReadModelNotificationsMode.Saga)
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

    public static class EventBusServiceExtensions
    {
        public static void SagaPublish<T>(this EventBusService eventBusService, T @event, CommandContext commandContext) where T : Event
        {
            eventBusService.Publish(@event, commandContext, ReadModelNotificationsMode.Saga);
        }

        public static void SagaPublish<T>(this EventBusService eventBusService, IEnumerable<Event> events, CommandContext commandContext) where T : Event
        {
            eventBusService.Publish(events, commandContext, ReadModelNotificationsMode.Saga);
        }
    }
}
