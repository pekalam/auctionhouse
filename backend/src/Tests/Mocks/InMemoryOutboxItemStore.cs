using Common.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalTests.Mocks
{
    internal class InMemoryOutboxItemStore : IOutboxItemStore
    {
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
}
