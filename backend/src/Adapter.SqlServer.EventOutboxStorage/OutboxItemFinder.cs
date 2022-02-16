using Common.Application.Events;
using Microsoft.EntityFrameworkCore;

namespace Adapter.SqlServer.EventOutbox
{
    internal class OutboxItemFinder : IOutboxItemFinder
    {
        private readonly EventOutboxDbContext _dbContext;

        public OutboxItemFinder(EventOutboxDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> GetTotalUnprocessedItemsOlderThan(long diff, long currentTimestamp)
        {
            return _dbContext.OutboxItems.CountAsync(i => !i.Processed && currentTimestamp - i.Timestamp > diff);
        }

        public async Task<IEnumerable<OutboxItem>> GetUnprocessedItemsOlderThan(long diff, long currentTimestamp, int limit)
        {
            var items = await _dbContext.OutboxItems
                .Where(i => !i.Processed && currentTimestamp - i.Timestamp > diff)
                .ToListAsync();
            return items.Select(DbOutboxItemAssembler.FromDbOutboxStoreItem);
        }
    }
}