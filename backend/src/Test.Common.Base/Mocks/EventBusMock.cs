using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common.Base.Mocks
{
    public class EventBusMock : IEventBus
    {
        public static EventBusMock Instance => new EventBusMock();


        public event Action<EventArgs, ILogger> Disconnected;

        public List<Event> PublishedEvents { get; private set; } = new();


        public Task Publish<T>(IAppEvent<T> @event) where T : Event
        {
            PublishedEvents.Add(@event.Event);
            return Task.CompletedTask;
        }

        public Task Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event
        {
            PublishedEvents.AddRange(@events.Select(e => e.Event));
            return Task.CompletedTask;
        }
    }
}
