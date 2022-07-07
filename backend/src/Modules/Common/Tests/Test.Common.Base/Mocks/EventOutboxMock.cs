using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Tests.Base.Mocks
{
    public class EventOutboxMock : IEventOutbox, IEventOutboxSavedItems
    {
        private readonly List<OutboxItem> _outboxItems = new();

        public IReadOnlyList<OutboxItem> SavedOutboxStoreItems => _outboxItems;

        public Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var item = new OutboxItem
            {
                CommandContext = commandContext,
                Event = @event,
                ReadModelNotifications = notificationsMode,
            };
            _outboxItems.Add(item);
            return Task.FromResult(item);
        }

        public Task<OutboxItem[]> SaveEvents(IEnumerable<Event> events, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var outboxItems = events.Select(e => new OutboxItem
            {
                CommandContext = commandContext,
                Event = e,
                ReadModelNotifications = notificationsMode,
            }).ToArray();
            _outboxItems.AddRange(outboxItems);
            return Task.FromResult(outboxItems);
        }
    }
}
