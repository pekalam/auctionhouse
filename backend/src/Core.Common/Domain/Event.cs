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
}