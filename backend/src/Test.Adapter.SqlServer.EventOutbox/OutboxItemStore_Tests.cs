using Adapter.SqlServer.EventOutbox;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Test.Common.Base.Builders;
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
            dbContext.OutboxItems.Add(new DbOutboxItem
            {
                CommandContext = CommandContext.CreateNew("tes"),
                Event = SerializationUtils.ToJson(new TestEvent()),
                ReadModelNotifications = ReadModelNotificationsMode.Disabled,
                Timestamp = 1,
            });
            await dbContext.SaveChangesAsync();

            var total = await outboxItemFinder.GetTotalUnprocessedItemsOlderThan(1, 3);
            Assert.Equal(1, total);
        }

        [Fact]
        public async Task g()
        {
            var dbStoreItem = new DbOutboxItem
            {
                CommandContext = CommandContext.CreateNew("tes"),
                Event = SerializationUtils.ToJson(new TestEvent()),
                ReadModelNotifications = ReadModelNotificationsMode.Disabled,
                Timestamp = 1,
            };
            dbContext.OutboxItems.Add(dbStoreItem);
            await dbContext.SaveChangesAsync();

            var unprocessed = (await outboxItemFinder.GetUnprocessedItemsOlderThan(1, 3, 10)).ToList();

            Assert.Single(unprocessed);
            Assert.Equal(unprocessed[0].CommandContext, dbStoreItem.CommandContext);
        }
    }

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
                ReadModelNotifications = ReadModelNotificationsMode.Immediate,
                Timestamp = 1,
            };
            var savedIitem = await outboxItemStore.Save(outboxStoreItem);

            Assert.NotEqual(0, savedIitem.Id);
            var dbOutboxStoreItem = await dbContext.OutboxItems.FirstAsync();
            Assert.Equal(JsonSerializer.Serialize(outboxStoreItem.CommandContext), JsonSerializer.Serialize(dbOutboxStoreItem.CommandContext));
            Assert.Equal(outboxStoreItem.ReadModelNotifications, dbOutboxStoreItem.ReadModelNotifications);
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
                ReadModelNotifications = ReadModelNotificationsMode.Immediate,
                Timestamp = 1,
            };
            var savedIitem = await outboxItemStore.Save(outboxStoreItem);

            savedIitem.Processed = true;
            await outboxItemStore.Update(savedIitem);

            Assert.NotEqual(0, savedIitem.Id);
            var dbOutboxStoreItem = await dbContext.OutboxItems.FirstAsync(i => i.Id == savedIitem.Id);
            Assert.Equal(JsonSerializer.Serialize(outboxStoreItem.CommandContext), JsonSerializer.Serialize(dbOutboxStoreItem.CommandContext));
            Assert.Equal(outboxStoreItem.ReadModelNotifications, dbOutboxStoreItem.ReadModelNotifications);
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
                Assert.Equal(JsonSerializer.Serialize(outboxStoreItems[i].CommandContext), JsonSerializer.Serialize(dbOutboxStoreItem.CommandContext));
                Assert.Equal(outboxStoreItems[i].ReadModelNotifications, dbOutboxStoreItem.ReadModelNotifications);
                Assert.Equal(outboxStoreItems[i].Timestamp, dbOutboxStoreItem.Timestamp);
                var deserializedEvent = SerializationUtils.FromJson(dbOutboxStoreItem.Event);
                Assert.Equal(outboxStoreItems[i].Event.GetType(), deserializedEvent.GetType());
                Assert.True(dbOutboxStoreItem.Processed);
            }
        }
    }
}