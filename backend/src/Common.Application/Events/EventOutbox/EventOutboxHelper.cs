using Common.Application.Commands;
using Core.Common.Domain;
using System.Diagnostics;

namespace Common.Application.Events
{
    public class EventOutboxHelper
    {
        private readonly IOutboxItemFinder _outboxItemFinder;
        private readonly IOutboxItemStore _outboxItemStore;
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly IEventBus _eventBus;

        private List<IAppEvent<Event>>? _appEvents;
        private OutboxItem[]? _outboxItems;

        public EventOutboxHelper()
        {
        }

        public async Task SaveEvents(IEnumerable<Event> events, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            _appEvents = new List<IAppEvent<Event>>(10);

            foreach (var @event in events)
            {
                _appEvents.Add(_appEventBuilder.WithCommandContext(commandContext)
                    .WithEvent(@event)
                    .WithReadModelNotificationsMode(notificationsMode)
                    .Build<Event>());
            }

            _outboxItems = _appEvents.Select(e => OutboxItem.CreateNew(e, false)).ToArray();
            await _outboxItemStore.SaveMany(_outboxItems);
        }

        public async Task SendEvents()
        {
            Debug.Assert(_outboxItems != null && _appEvents != null);
            _eventBus.Publish(_appEvents);
            foreach (var item in _outboxItems)
            {
                item.Processed = true;
            }
            await _outboxItemStore.UpdateMany(_outboxItems);
        }
    }
}
