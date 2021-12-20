using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Common.Domain
{
    public abstract class AggregateRoot<T, U> where T : AggregateRoot<T, U>, new() where U : UpdateEventGroup
    {
        private bool _canAddNewEvents = true;
        private List<Event> _pendingEvents = new List<Event>();
        public IReadOnlyCollection<Event> PendingEvents => _pendingEvents;
        public long Version { get; private set; } = 0;
        private U _updateEventGroup = null;

        public static T FromEvents(IReadOnlyCollection<Event> events)
        {
            T t = new T();
            t._canAddNewEvents = false;
            foreach (var @event in events)
            {
                t.Apply(@event);
            }

            if (events.Count > 0)
            {
                t.Version = events.Last().AggVersion;
            }

            t._canAddNewEvents = true;
            return t;
        }

        public void MarkPendingEventsAsHandled()
        {
            _pendingEvents.Clear();
        }

        protected void AddEvent(Event @event)
        {
            if (_canAddNewEvents)
            {
                @event.AggVersion = ++Version;
                _pendingEvents.Add(@event);
            }
        }

        protected void AddUpdateEvent(UpdateEvent @event)
        {
            if (_canAddNewEvents)
            {
                if (_updateEventGroup == null)
                {
                    _updateEventGroup = CreateUpdateEventGroup();
                    _updateEventGroup.AggVersion = ++Version;
                    _updateEventGroup.AggregateId = AggregateId;
                    _pendingEvents.Add(_updateEventGroup);
                }
                _updateEventGroup.Add(@event);
            }
        }

        protected abstract void Apply(Event @event);

        protected abstract U CreateUpdateEventGroup();

        public Guid AggregateId { get; protected set; } = Guid.NewGuid();
    }
}