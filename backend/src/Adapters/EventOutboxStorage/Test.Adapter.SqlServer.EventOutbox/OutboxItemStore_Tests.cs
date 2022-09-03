using Adapter.SqlServer.EventOutbox;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Tests.Base.Builders;
using Xunit;

namespace Test.Adapter.SqlServer.EventOutbox
{
    [Trait("Category", "Integration")]
    public class OutboxItemStore_Tests
    {
        EventOutboxDbContext dbContext;
        IOutboxItemStore outboxItemStore;

        public OutboxItemStore_Tests()
        {
            dbContext = new EventOutboxDbContext(new DbContextOptionsBuilder<EventOutboxDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            outboxItemStore = new OutboxItemStore(dbContext);
        }

        public class TestEvent : Event
        {
            public TestEvent() : base("TestEvent")
            {
            }
        }

        [Fact]
        public async Task Test1Async()
        {
            var outboxStoreItem = new OutboxItem
            {
                CommandContext = CommandContext.CreateNew(""),
                Event = new TestEvent(),
                Timestamp = 1,
            };
            var savedIitem = await outboxItemStore.Save(outboxStoreItem);

            Assert.NotEqual(0, savedIitem.Id);
            var dbOutboxStoreItem = await dbContext.OutboxItems.FirstAsync();
            outboxStoreItem.CommandContext.AssertCommandContextIsEqualTo(dbOutboxStoreItem.CommandContext);
            Assert.Equal(outboxStoreItem.Timestamp, dbOutboxStoreItem.Timestamp);
            var deserializedEvent = SerializationUtils.FromJson(dbOutboxStoreItem.Event);
            Assert.Equal(outboxStoreItem.Event.GetType(), deserializedEvent.GetType());
        }

        [Fact]
        public async Task Test2Async()
        {
            var outboxStoreItem = new OutboxItem
            {
                CommandContext = CommandContext.CreateNew(""),
                Event = new TestEvent(),
                Timestamp = 1,
            };
            var savedIitem = await outboxItemStore.Save(outboxStoreItem);

            savedIitem.Processed = true;
            await outboxItemStore.Update(savedIitem);

            Assert.NotEqual(0, savedIitem.Id);
            var dbOutboxStoreItem = await dbContext.OutboxItems.FirstAsync(i => i.Id == savedIitem.Id);
            outboxStoreItem.CommandContext.AssertCommandContextIsEqualTo(dbOutboxStoreItem.CommandContext);
            Assert.Equal(outboxStoreItem.Timestamp, dbOutboxStoreItem.Timestamp);
            var deserializedEvent = SerializationUtils.FromJson(dbOutboxStoreItem.Event);
            Assert.Equal(outboxStoreItem.Event.GetType(), deserializedEvent.GetType());
            Assert.True(dbOutboxStoreItem.Processed);
        }


        [Fact]
        public async Task Test3Async()
        {
            var outboxStoreItems = Enumerable.Range(0, 2).Select((_) => new GivenOutboxStoreItem().Build()).ToArray();
            outboxStoreItems[1].CommandContext = outboxStoreItems[0].CommandContext;
            var savedIitems = (await outboxItemStore.SaveMany(outboxStoreItems)).ToArray();

            foreach (var item in savedIitems)
            {
                item.Processed = true;
            }
            await outboxItemStore.UpdateMany(savedIitems);

            for (int i = 0; i < savedIitems.Length; i++)
            {
                Assert.NotEqual(0, savedIitems[i].Id);
                var savedItem = savedIitems[i];
                var dbOutboxStoreItem = await dbContext.OutboxItems.FirstAsync(i => i.Id == savedItem.Id);
                outboxStoreItems[i].CommandContext.AssertCommandContextIsEqualTo(dbOutboxStoreItem.CommandContext);
                Assert.Equal(outboxStoreItems[i].Timestamp, dbOutboxStoreItem.Timestamp);
                var deserializedEvent = SerializationUtils.FromJson(dbOutboxStoreItem.Event);
                Assert.Equal(outboxStoreItems[i].Event.GetType(), deserializedEvent.GetType());
                Assert.True(dbOutboxStoreItem.Processed);
            }
        }
    }
}