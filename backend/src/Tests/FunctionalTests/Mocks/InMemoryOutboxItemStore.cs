﻿using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionalTests.Mocks
{
    internal class InMemoryOutboxItemStore : IOutboxItemStore
    {
        public static InMemoryOutboxItemStore Instance { get; private set; } = Create();

        public static InMemoryOutboxItemStore Create() => new InMemoryOutboxItemStore();

        public Task<OutboxItem> Save(OutboxItem item)
        {
            return Task.FromResult(item);
        }

        public Task<IEnumerable<OutboxItem>> SaveMany(IEnumerable<OutboxItem> items)
        {
            return Task.FromResult(items);
        }

        public Task Update(OutboxItem item)
        {
            return Task.CompletedTask;
        }

        public Task UpdateMany(IEnumerable<OutboxItem> items)
        {
            return Task.CompletedTask;
        }
    }

    internal class InMemoryEventBusDecorator : IEventBus
    {
        internal readonly IEventBus _eventBus;
        internal readonly SemaphoreSlim _sem = new(1,1);

        public InMemoryEventBusDecorator(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public event Action<EventArgs, ILogger> Disconnected = null!;

        private static List<IAppEvent<Event>> _sentEvents = new();
        public static IReadOnlyList<IAppEvent<Event>> SentEvents => _sentEvents;

        public static void ClearSentEvents() => _sentEvents.Clear();

        public async Task Publish<T>(IAppEvent<T> @event) where T : Event
        {
            _sem.Wait();
            try
            {
                _sentEvents.Add(@event);
                await _eventBus.Publish(@event);
            }
            finally
            {
                _sem.Release();
            }
        }

        public async Task Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event
        {
            _sem.Wait();
            try
            {
                _sentEvents.AddRange(events);
                await _eventBus.Publish(events);
            }
            finally
            {
                _sem.Release();
            }
        }
    }
}
