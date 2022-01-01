using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Common.Domain
{
    namespace Default
    {
        public abstract class AggregateRoot<T, U> : AggregateRoot<T, Guid, U>
            where T : AggregateRoot<T, Guid, U>, new() where U : UpdateEventGroup
        {

        }


        public abstract class AggregateRoot<T> : Domain.AggregateRoot<T, Guid> where T : Domain.AggregateRoot<T, Guid>, new()
        {
        }
    }

    public abstract class AggregateRoot<T, TId, U> : AggregateRoot<T, TId> where T : AggregateRoot<T, TId, U>, new() where U : UpdateEventGroup
    {
        private U _updateEventGroup = null;

        protected void AddUpdateEvent(UpdateEvent @event)
        {
            if (_canAddNewEvents)
            {
                if (_updateEventGroup == null)
                {
                    _updateEventGroup = CreateUpdateEventGroup();
                    _updateEventGroup.AggVersion = ++Version;
                    _updateEventGroup.AggregateId = Guid.Parse(AggregateId.ToString()); //TODO
                    _pendingEvents.Add(_updateEventGroup);
                }
                _updateEventGroup.Add(@event);
            }
        }

        protected abstract U CreateUpdateEventGroup();
    }

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

        protected void AddEvent(Event @event)
        {
            if (_canAddNewEvents)
            {
                @event.AggVersion = ++Version;
                _pendingEvents.Add(@event);
            }
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