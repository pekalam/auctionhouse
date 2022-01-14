using Common.Application.Commands;
using Core.Common.Domain;
using System.Diagnostics;

namespace Common.Application.Events
{
    /// <summary>
    /// Sends events from outbox and marks them as processed.
    /// </summary>
    public class EventOutboxSender
    {
        private readonly IOutboxItemStore _outboxItemStore;
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly IEventBus _eventBus;

        public EventOutboxSender(IOutboxItemStore outboxItemStore, IAppEventBuilder appEventBuilder, IEventBus eventBus)
        {
            _outboxItemStore = outboxItemStore;
            _appEventBuilder = appEventBuilder;
            _eventBus = eventBus;
        }

        public async Task SendEvents(IEnumerable<OutboxItem> outboxItems)
        {
            var appEvents = outboxItems
                .Select(i => _appEventBuilder
                    .WithCommandContext(i.CommandContext)
                    .WithReadModelNotificationsMode(i.ReadModelNotifications)
                    .WithEvent(i.Event)
                    .Build<Event>());

            _eventBus.Publish(appEvents);
            foreach (var item in outboxItems)
            {
                item.Processed = true;
            }
            await _outboxItemStore.UpdateMany(outboxItems);
        }
    }
}
