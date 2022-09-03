using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using ReadModelNotifications.SagaNotifications;

namespace ReadModelNotifications.EventOutbox
{



    /// <summary>
    /// Adds saga event notifications as part of process of saving event in outbox.
    /// It transparently adds notifications events when command is saving events. //TODO make this process explicit
    /// </summary>
    internal class CommandNotificationsEventOutboxWrapper : IEventOutbox
    {
        private readonly IEventOutbox _eventOutbox;
        private readonly IEventOutboxSavedItems _eventOutboxSavedItems;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;

        public CommandNotificationsEventOutboxWrapper(IEventOutbox eventOutbox, IEventOutboxSavedItems eventOutboxSavedItems, Lazy<ISagaNotifications> sagaNotifications)
        {
            _eventOutbox = eventOutbox;
            _eventOutboxSavedItems = eventOutboxSavedItems;
            _sagaNotifications = sagaNotifications;
        }

        public async Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            //TODO: should every saved event be added to saga unhandled events? - make distinction between part of saga and initiator
            var item = await _eventOutbox.SaveEvent(@event, commandContext, notificationsMode);
            commandContext.SetNotificationsMode(notificationsMode);
            await AddSagaUnhandledEvents(commandContext);
            return item;
        }

        public async Task<OutboxItem[]> SaveEvents(IEnumerable<Event> @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var items = await _eventOutbox.SaveEvents(@event, commandContext, notificationsMode);
            commandContext.SetNotificationsMode(notificationsMode);
            await AddSagaUnhandledEvents(commandContext);
            return items;
        }

        private async Task AddSagaUnhandledEvents(CommandContext commandContext)
        {
            var eventsWithSagaNotificationsMode = _eventOutboxSavedItems.SavedOutboxStoreItems
                .Where(i => i.CommandContext.GetNotificationsMode() == ReadModelNotificationsMode.Saga)
                .Select(i => i.Event)
                .ToArray();
            if (eventsWithSagaNotificationsMode.Length > 0)
            {
                await _sagaNotifications.Value.AddUnhandledEvents(commandContext.CorrelationId, eventsWithSagaNotificationsMode);
            }
        }
    }
}
