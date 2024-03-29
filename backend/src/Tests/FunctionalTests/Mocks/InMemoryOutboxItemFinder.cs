﻿using Common.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalTests.Mocks
{
    internal class InMemoryOutboxItemFinder : IOutboxItemFinder
    {
        public static InMemoryOutboxItemFinder Instance { get; private set; } = Create();

        public static InMemoryOutboxItemFinder Create() => new InMemoryOutboxItemFinder();

        public Task<int> GetTotalUnprocessedItemsOlderThan(long diff, long currentTimestamp)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<OutboxItem>> GetUnprocessedItemsOlderThan(long diff, long currentTimestamp, int limit)
        {
            return Task.FromResult(Enumerable.Empty<OutboxItem>());
        }

        public Task MarkAsProcessed(OutboxItem item)
        {
            return Task.CompletedTask;
        }

        public Task MarkAsProcessed(IEnumerable<OutboxItem> items)
        {
            return Task.CompletedTask;
        }
    }
}
