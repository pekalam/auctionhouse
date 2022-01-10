using Common.Application.Commands;
using Common.Application.SagaNotifications;
using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Events
{
    /// <summary>
    /// Implements in memory outbox
    /// </summary>
    public class EventBusFacadeWithOutbox : EventBusFacade //TODO interface + compositon
    {
        private readonly List<List<Event>> _eventsToPublish = new(10);
        private readonly List<CommandContext> _commandContext = new(5);
        private readonly List<ReadModelNotificationsMode> _notificationsMode = new(5);

        public EventBusFacadeWithOutbox(IEventBus eventBus, IAppEventBuilder appEventBuilder, Lazy<ISagaNotifications> sagaNotificationsFactory) : base(eventBus, appEventBuilder, sagaNotificationsFactory)
        {
        }

        public override void Publish(IEnumerable<Event> events, CommandContext commandContext, ReadModelNotificationsMode consistencyMode)
        {
            var eventList = new List<Event>(10);
            eventList.AddRange(events);
            _eventsToPublish.Add(eventList);
            _commandContext.Add(commandContext);
            _notificationsMode.Add(consistencyMode);
        }

        public override void Publish<T>(T @event, CommandContext commandContext, ReadModelNotificationsMode consistencyMode)
        {
            var eventList = new List<Event>(1);
            eventList.Add(@event);
            _eventsToPublish.Add(eventList);
            _commandContext.Add(commandContext);
            _notificationsMode.Add(consistencyMode);
        }

        public void ProcessOutbox()
        {
            int i = 0;
            foreach (var eventList in _eventsToPublish)
            {
                base.Publish(eventList, _commandContext[i], _notificationsMode[i]);
                i++;
            }
            _eventsToPublish.Clear();
            _commandContext.Clear();
            _notificationsMode.Clear();
        }
    }
}
