﻿using Common.Application.Commands;
using Core.Common.Domain;
using System.Diagnostics;

namespace Common.Application.Events
{
    /// <summary>
    /// Sends events from outbox and marks them as processed.
    /// </summary>
    public class EventOutboxItemsSender
    {
        private readonly IOutboxItemStore _outboxItemStore;
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly IEventBus _eventBus;

        public EventOutboxItemsSender(IOutboxItemStore outboxItemStore, IAppEventBuilder appEventBuilder, IEventBus eventBus)
        {
            _outboxItemStore = outboxItemStore;
            _appEventBuilder = appEventBuilder;
            _eventBus = eventBus;
        }

        public async Task Send(IEnumerable<OutboxItem> outboxItems)
        {
            var appEvents = outboxItems
                .Select(i => _appEventBuilder
                    .WithCommandContext(i.CommandContext)
                    .WithEvent(i.Event)
                    .Build<Event>());

            await _eventBus.Publish(appEvents);
            await _outboxItemStore.UpdateMany(outboxItems.Select(static i =>
            {
                i.Processed = true;
                return i;
            }));
        }
    }
}
