using Adapter.SqlServer.EventOutbox;
using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Test.Adapter.SqlServer.EventOutbox
{
    [Trait("Category", "Integration")]
    public class OutboxItemFinder_Tests
    {
        EventOutboxDbContext dbContext;
        OutboxItemFinder outboxItemFinder;

        public OutboxItemFinder_Tests()
        {
            dbContext = new EventOutboxDbContext(new DbContextOptionsBuilder<EventOutboxDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            outboxItemFinder = new OutboxItemFinder(dbContext);
        }

        public class TestEvent : Event
        {
            public TestEvent() : base("TestEvent")
            {
            }
        }

        [Fact]
        public async Task f()
        {
            dbContext.OutboxItems.Add(CreateDbOutboxItem(timestamp: 1));
            await dbContext.SaveChangesAsync();

            var total = await outboxItemFinder.GetTotalUnprocessedItemsOlderThan(1, 3);
            Assert.Equal(1, total);
        }

        private DbOutboxItem CreateDbOutboxItem(int timestamp)
        {
            return new DbOutboxItem
            {
                CommandContext = new DbCommandContext
                {
                    CommandId = "123",
                    CorrelationId = "123",
                    Name = "test",
                    User = Guid.NewGuid(),
                    HttpQueued = false,
                    WSQueued = false
                },
                Event = SerializationUtils.ToJson(new TestEvent()),
                Timestamp = timestamp,
            };
        }

        [Fact]
        public async Task g()
        {
            var dbStoreItem = CreateDbOutboxItem(timestamp: 1);
            dbContext.OutboxItems.Add(dbStoreItem);
            await dbContext.SaveChangesAsync();

            var unprocessed = (await outboxItemFinder.GetUnprocessedItemsOlderThan(1, 3, 10)).ToList();

            Assert.Single(unprocessed);
            unprocessed[0].CommandContext.AssertCommandContextIsEqualTo(dbStoreItem.CommandContext);
        }
    }
}