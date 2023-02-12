using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using ReadModelNotifications.SagaNotifications;
using ReadModelNotifications.Settings;

namespace ReadModelNotifications.EventOutbox
{



    /// <summary>
    /// Adds saga event notifications as part of process of saving event in outbox.
    /// It transparently adds notifications events when command is saving events. //TODO make this process explicit
    /// </summary>
    internal class CommandNotificationsEventOutboxWrapper : IEventOutbox
    {
        private readonly IEventOutbox _eventOutbox;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly ReadModelNotificationsSettings _settings;

        public CommandNotificationsEventOutboxWrapper(IEventOutbox eventOutbox, Lazy<ISagaNotifications> sagaNotifications, ReadModelNotificationsSettings settings)
        {
            _eventOutbox = eventOutbox;
            _sagaNotifications = sagaNotifications;
            _settings = settings;
        }

        public IReadOnlyList<OutboxItem> SavedOutboxStoreItems => _eventOutbox.SavedOutboxStoreItems;

        public async Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext)
        {
            var item = await _eventOutbox.SaveEvent(@event, commandContext);
            await AddSagaUnhandledEvents(commandContext, new[] {@event});
            return item;
        }

        public async Task<OutboxItem[]> SaveEvents(IEnumerable<Event> events, CommandContext commandContext)
        {
            var items = await _eventOutbox.SaveEvents(events, commandContext);
            await AddSagaUnhandledEvents(commandContext, events);
            return items;
        }

        private async Task AddSagaUnhandledEvents(CommandContext commandContext, IEnumerable<Event> events)
        {
            var eventsWithSagaNotificationsMode = events
                .Where(e => _settings.IsEventToConfirmInSaga(commandContext, e.EventName))
                .ToArray();
            if (eventsWithSagaNotificationsMode.Length > 0)
            {
                await _sagaNotifications.Value.AddUnhandledEvents(commandContext.CorrelationId, eventsWithSagaNotificationsMode);
            }
        }
    }
}
