using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;

namespace Common.Tests.Base.Mocks
{
    public class EventOutboxMock : IEventOutbox, IEventOutboxSavedItems
    {
        public static EventOutboxMock Instance { get; } = new EventOutboxMock();

        private readonly List<OutboxItem> _outboxItems = new();

        public IReadOnlyList<OutboxItem> SavedOutboxStoreItems => _outboxItems;

        public Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext)
        {
            var item = new OutboxItem
            {
                CommandContext = commandContext,
                Event = @event,
            };
            _outboxItems.Add(item);
            return Task.FromResult(item);
        }

        public Task<OutboxItem[]> SaveEvents(IEnumerable<Event> events, CommandContext commandContext)
        {
            var outboxItems = events.Select(e => new OutboxItem
            {
                CommandContext = commandContext,
                Event = e,
            }).ToArray();
            _outboxItems.AddRange(outboxItems);
            return Task.FromResult(outboxItems);
        }
    }
}
