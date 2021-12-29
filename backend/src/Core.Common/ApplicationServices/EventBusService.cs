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

        public EventBusService(IEventBus eventBus, IAppEventBuilder appEventBuilder)
        {
            _eventBus = eventBus;
            _appEventBuilder = appEventBuilder;
        }

        public virtual void Publish<T>(T @event, CommandContext commandContext) where T : Event
        {
            var appEvent = _appEventBuilder
                .WithCommandContext(commandContext)
                .WithEvent(@event)
                .Build<T>();
            _eventBus.Publish(appEvent);
        }

        public virtual void Publish(IEnumerable<Event> events, CommandContext commandContext)
        {
            foreach (var @event in events)
            {
                Publish(@event, commandContext);
            }
        }
    }
}
