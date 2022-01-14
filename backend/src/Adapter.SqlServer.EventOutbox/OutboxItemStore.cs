using Common.Application.Events;
using Microsoft.EntityFrameworkCore;

namespace Adapter.SqlServer.EventOutbox
{
    internal class OutboxItemStore : IOutboxItemStore
    {
        private readonly EventOutboxDbContext _dbContext;

        public OutboxItemStore(EventOutboxDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OutboxItem> Save(OutboxItem item)
        {
            var dbStoreItem = DbOutboxItemAssembler.ToDbOutboxStoreItem(item);

            _dbContext.OutboxItems.Add(dbStoreItem);
            await _dbContext.SaveChangesAsync();

            item.Id = dbStoreItem.Id;
            return item;
        }

        public async Task<IEnumerable<OutboxItem>> SaveMany(IEnumerable<OutboxItem> items)
        {
            var dbItems = new List<DbOutboxItem>(10);
            foreach (var item in items)
            {
                var dbStoreItem = DbOutboxItemAssembler.ToDbOutboxStoreItem(item);
                _dbContext.OutboxItems.Add(dbStoreItem);
                dbItems.Add(dbStoreItem);
            }
            await _dbContext.SaveChangesAsync();

            int i = 0;
            foreach (var item in items)
            {
                item.Id = dbItems[i++].Id;
            }

            return items;
        }

        private void UpdateItem(OutboxItem item)
        {
            var local = _dbContext.OutboxItems.Local.FirstOrDefault(i => i.Id == item.Id);
            var dbOutboxItem = DbOutboxItemAssembler.ToDbOutboxStoreItem(item);
            if (local != null)
            {
                _dbContext.Entry(local).State = EntityState.Detached;
                _dbContext.Entry(dbOutboxItem).State = EntityState.Modified;
            }
            else
            {
                _dbContext.OutboxItems.Update(dbOutboxItem);
            }
        }

        public async Task Update(OutboxItem item)
        {
            UpdateItem(item);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateMany(IEnumerable<OutboxItem> items)
        {
            foreach (var item in items)
            {
                UpdateItem(item);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}