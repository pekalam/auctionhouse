using System;
using System.Collections.Generic;

namespace Core.Common.Domain
{
    public class Event
    {
        public string EventName { get; }
        public virtual long AggVersion { get; set; }

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
        override public long AggVersion { get => -1; set { } } // agg versions is contained in UpdateEventGroup in which this UpdateEvent is contained

        public UpdateEvent(string eventName) : base(eventName)
        {
        }
    }

    /// <summary>
    /// Represents list of update events from aggregate. These are events which can be sent together as single <see cref="UpdateEventGroup"/>
    /// instead of being sent separately.
    /// </summary>
    public class UpdateEventGroup<TId> : Event
    {
        public TId AggregateId { get; set; }
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