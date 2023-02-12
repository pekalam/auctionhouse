using Common.Application.Events;
using Common.Application.Tests.Mocks;
using Common.Application.Tests.ServiceContracts;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Tests.Base.Builders;
using Common.Tests.Base.Mocks.Events;
using Xunit;

namespace Common.Application.Tests
{
    public class Timestamp
    {
        public long Value { get; }

        private Timestamp(long value)
        {
            Value = value;
        }

        public static Timestamp FromMiliseconds(long miliseconds)
        {
            return new Timestamp(miliseconds * (1_000_000 / 100));
        }

        public static Timestamp Now() => new Timestamp(SysTime.Now.ToFileTime());
    }

    [Trait("Category", "Unit")]
    public class EventOutboxProcessor_Tests
    {
        [Fact]
        public async Task Handles_events_if_their_timestamp_exceeds_minimum()
        {
            //setup saved outbox items
            var savedOutboxStoreItems = GivenSavedOutboxStoreItems();
            //setup current timestamp
            var currentTimestamp = GivenMockCurrentTimestamp();
            // set min miliseconds diff
            var minMilisecondsDiff = 1000;
            // setup EventOutboxPostProcessor dependencies
            var eventBus = EventBusMock.Instance;
            var outboxItemStore = OutboxItemStoreMock.Instance;
            var outboxItemFinder = GivenMockOutboxItemFinder(savedOutboxStoreItems, currentTimestamp, minMilisecondsDiff);
            // setup service
            var outboxProcessor = GivenOutboxProcessor(minMilisecondsDiff, eventBus, outboxItemFinder, outboxItemStore);

            await outboxProcessor.ProcessEvents();

            eventBus.Verify(f => f.Publish(It.Is((IEnumerable<IAppEvent<Event>> e) => e.Count() == StoreItemsToBeHandled)), Times.Once());
            outboxItemStore.Verify(f => f.UpdateMany(It.Is
                ((IEnumerable<OutboxItem> e) => e.SequenceEqual(savedOutboxStoreItems.Take(StoreItemsToBeHandled)) && e.All(i => i.Processed))), Times.Once());
        }

        private static EventOutboxProcessor GivenOutboxProcessor(int minMilisecondsDiff, Mock<IEventBus> eventBus, Mock<IOutboxItemFinder> outboxItemFinder, Mock<IOutboxItemStore> outboxItemStore)
        {
            return new EventOutboxProcessor(
                            new() { MinMilisecondsDiff = minMilisecondsDiff, },
                             outboxItemFinder.Object, Mock.Of<ILogger<EventOutboxProcessor>>(),
                            new EventOutboxItemsSender(outboxItemStore.Object, new TestAppEventBuilder(), eventBus.Object)
                            );
        }

        private static Mock<IOutboxItemFinder> GivenMockOutboxItemFinder(OutboxItem[] savedOutboxStoreItems, Timestamp currentTimestamp, int minMilisecondsDiff)
        {
            return OutboxItemFinderMock.Instance(
                                OutboxItemFinderContracts.ValidGetUnprocessedItemsOlderThanArgs(
                                    savedOutboxStoreItems,
                                Timestamp.FromMiliseconds(minMilisecondsDiff),
                                currentTimestamp,
                                StoreItemsToBeHandled),
                                OutboxItemFinderContracts.ValidGetTotalUnprocessedItemsOlder(
                                    Timestamp.FromMiliseconds(minMilisecondsDiff),
                                    currentTimestamp,
                                    StoreItemsToBeHandled
                                    )
                                );
        }

        private static Timestamp GivenMockCurrentTimestamp()
        {
            var now = SysTime.Now.AddSeconds(6);
            SysTime.DateTimeFactory = () => now;
            return Timestamp.Now();
        }

        private const int StoreItemsToBeHandled = 2;
        private const int StoreItemsToBeIgnored = 1;

        private static OutboxItem[] GivenSavedOutboxStoreItems()
        {
            // setup save store items
            var savedOutboxStoreItems = new[]
            {
                // to be sent
                new GivenOutboxStoreItem().WithTimestamp(SysTime.Now.ToFileTime()).Build(),
                new GivenOutboxStoreItem().WithTimestamp(SysTime.Now.AddSeconds(4).ToFileTime()).Build(),
                // ignored
                new GivenOutboxStoreItem().WithTimestamp(SysTime.Now.AddSeconds(5).ToFileTime()).Build(),
            };
            return savedOutboxStoreItems;
        }
    }
}