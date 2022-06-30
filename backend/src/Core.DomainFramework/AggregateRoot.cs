using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Common.Domain
{
    public abstract class AggregateRoot<T, TId> : IInternalEventAdd where T : AggregateRoot<T, TId>, new()
    {
        static AggregateRoot()
        {
            if (typeof(TId) == typeof(UpdateEventGroup))
            {
                throw new InvalidOperationException("Cannot pass UpdateEventGroup as type parameter. Use AggregateRoot from Default namespace");
            }
        }

        public TId AggregateId { get; protected set; }

        protected bool _canAddNewEvents = true;
        protected List<Event> _pendingEvents = new List<Event>();
        public IReadOnlyCollection<Event> PendingEvents => _pendingEvents;
        public long Version { get; protected set; } = 0;

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

        protected T AddEvent<T>(T @event) where T : Event
        {
            if (_canAddNewEvents)
            {
                @event.AggVersion = ++Version;
                _pendingEvents.Add(@event);
            }
            return @event;
        }

        protected abstract void Apply(Event @event);

        void IInternalEventAdd.AddEvent(Event @event)
        {
            if (_canAddNewEvents)
            {
                @event.AggVersion = ++Version;
                _pendingEvents.Add(@event);
            }
        }
    }
}