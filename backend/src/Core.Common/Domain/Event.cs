using System;
using System.Collections.Generic;

namespace Core.Common.Domain
{
    public class Event
    {
        public string EventName { get; }
        public long AggVersion { get; set; }

        public Event(string eventName)
        {
            EventName = eventName;
        }
    }

    /// <summary>
    /// Event that can be part of <see cref="UpdateEventGroup"/>
    /// </summary>
    public class UpdateEvent : Event
    {
        public UpdateEvent(string eventName) : base(eventName)
        {
            AggVersion = -1; // agg versions is contained in UpdateEventGroup in which this UpdateEvent is contained
        }
    }

    /// <summary>
    /// Represents list of update events from aggregate. These are events which can be sent together as single <see cref="UpdateEventGroup"/>
    /// instead of being sent separately.
    /// </summary>
    public class UpdateEventGroup : Event
    {
        public Guid AggregateId { get; set; }
        public List<UpdateEvent> UpdateEvents { get; set; }

        public UpdateEventGroup(string eventName) : base(eventName)
        {
            UpdateEvents = new List<UpdateEvent>();
        }

        public void Add(UpdateEvent @event)
        {
            UpdateEvents.Add(@event);
        }
    }
}