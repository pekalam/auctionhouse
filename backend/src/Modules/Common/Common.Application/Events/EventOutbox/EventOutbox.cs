using Common.Application.Commands;
using Core.Common.Domain;

namespace Common.Application.Events
{
    /// <summary>
    /// Saves events with <see cref="IOutboxItemStore"/>.
    /// </summary>
    internal class EventOutbox : IEventOutbox, IEventOutboxSavedItems
    {
        private readonly IOutboxItemStore _outboxStore;
        private readonly IAppEventBuilder _appEventBuilder;

        public EventOutbox(IOutboxItemStore outboxStore, IAppEventBuilder appEventBuilder)
        {
            _outboxStore = outboxStore;
            _appEventBuilder = appEventBuilder;
        }

        private List<OutboxItem> _outboxStoreItems = new();
        public IReadOnlyList<OutboxItem> SavedOutboxStoreItems => _outboxStoreItems;

        public async Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var appEvent = _appEventBuilder.WithCommandContext(commandContext)
                .WithEvent(@event)
                .Build<Event>();

            var outboxItem = OutboxItem.CreateNew(appEvent, false);
            _outboxStoreItems.Add(outboxItem);
            await _outboxStore.Save(outboxItem);
            return outboxItem;
        }

        public async Task<OutboxItem[]> SaveEvents(IEnumerable<Event> events, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var appEvents = new List<IAppEvent<Event>>(10);

            foreach (var @event in events)
            {
                appEvents.Add(_appEventBuilder.WithCommandContext(commandContext)
                    .WithEvent(@event)
                    .Build<Event>());
            }

            var outboxItems = appEvents.Select(e => OutboxItem.CreateNew(e, false)).ToArray();
            _outboxStoreItems.AddRange(outboxItems);
            await _outboxStore.SaveMany(outboxItems);
            return outboxItems;
        }
    }
}
