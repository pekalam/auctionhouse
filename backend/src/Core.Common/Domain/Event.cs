using System;
using System.Collections.Generic;

namespace Core.Common.Domain
{
    public class Event
    {
        public string EventName { get; protected set; }
        public long AggVersion { get; set; }

        public Event(string eventName)
        {
            EventName = eventName;
        }
    }

    public class UpdateEvent : Event
    {
        public UpdateEvent(string eventName) : base(eventName)
        {
            AggVersion = -1;
        }
    }

    public class UpdateEventGroup : Event
    {
        public Guid AggregateId { get; set; }
        public List<UpdateEvent> UpdateEvents { get; }

        public UpdateEventGroup(string eventName) : base(eventName)
        {
            UpdateEvents = new List<UpdateEvent>();
        }

        public UpdateEventGroup(string eventName, List<UpdateEvent> updateEvents) : base(eventName)
        {
            UpdateEvents = updateEvents;
        }

        public void Add(UpdateEvent @event)
        {
            UpdateEvents.Add(@event);
        }
    }
}