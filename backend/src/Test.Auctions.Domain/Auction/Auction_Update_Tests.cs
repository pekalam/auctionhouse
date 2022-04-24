using Auctions.DomainEvents;
using Auctions.DomainEvents.Update;
using Auctions.Tests.Base.Domain.ModelBuilders;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Auctions.Domain.Tests
{
    [Trait("Category", "Unit")]
    public class Auction_Update_Tests
    {
        private static Type UpdateAndAssertDescription(Auction auction)
        {
            auction.UpdateDescription("description");

            auction.Product.Description.Should().BeEquivalentTo("description");
            return typeof(AuctionDescriptionChanged);
        }

        private static Type UpdateAndAssertTags(Auction auction)
        {
            var newTags = new[]
            {
                new Tag("testTag1"),
            };

            auction.UpdateTags(newTags);

            auction.Tags.Should().BeEquivalentTo(newTags);
            return typeof(AuctionTagsChanged);
        }

        private static Type UpdateBuyNowPrice(Auction auction)
        {
            var newBuyNowPrice = new BuyNowPrice(100);

            auction.UpdateBuyNowPrice(newBuyNowPrice);

            auction.BuyNowPrice.Should().BeEquivalentTo(newBuyNowPrice);
            return typeof(AuctionBuyNowPriceChanged);
        }

        private static Type UpdateName(Auction auction)
        {
            var newName = auction.Name + " test";

            auction.UpdateName(newName);

            auction.Name.Value.Should().Be(newName);
            return typeof(AuctionNameChanged);
        }

        private static Func<Auction, Type> CreateUpdateAndAssertDelegate(string methodName)
        {
            var method = typeof(Auction_Update_Tests)
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .First(m => m.Name == methodName);

            return (auction) => (Type)method.Invoke(null, new[] { auction });
        }

        [Theory]
        [InlineData(nameof(UpdateAndAssertDescription))]
        [InlineData(nameof(UpdateAndAssertTags))]
        [InlineData(nameof(UpdateBuyNowPrice))]
        [InlineData(nameof(UpdateName))]
        public void Can_be_updated_with_update_methods_and_emits_valid_updateEventGroup(string updateAndAssertMethodName)
        {
            var updateAndAssert = CreateUpdateAndAssertDelegate(updateAndAssertMethodName);
            var auction = new GivenAuction().Build();
            auction.MarkPendingEventsAsHandled();

            var eventType = updateAndAssert(auction);

            auction.PendingEvents.Should().HaveCount(1);
            auction.PendingEvents.ElementAt(0).Should().BeOfType<AuctionUpdateEventGroup>();
            var eventGroup = auction.PendingEvents.First() as AuctionUpdateEventGroup;
            eventGroup.UpdateEvents.Should().HaveCount(1);
            eventGroup.UpdateEvents.First().Should().BeOfType(eventType);
        }

        [Theory]
        [InlineData(nameof(UpdateAndAssertDescription) + "," + nameof(UpdateAndAssertTags) + "," + nameof(UpdateBuyNowPrice) + "," + nameof(UpdateName))]
        public void Can_be_updated_with_update_methods_and_update_events_are_added_to_one_updateEventGroup(string updateAndAssertMethodNames)
        {
            var updateAndAssertDelegates = updateAndAssertMethodNames.Split(',').Select(CreateUpdateAndAssertDelegate).ToArray();
            var auction = new GivenAuction().Build();
            auction.MarkPendingEventsAsHandled();

            var expectedEventsInEventGroup = 0;
            foreach (var updateAndAssert in updateAndAssertDelegates)
            {
                var eventType = updateAndAssert(auction);
                expectedEventsInEventGroup++;

                auction.PendingEvents.Should().HaveCount(1);
                auction.PendingEvents.ElementAt(0).Should().BeOfType<AuctionUpdateEventGroup>();
                var eventGroup = auction.PendingEvents.First() as AuctionUpdateEventGroup;
                eventGroup.UpdateEvents.Should().HaveCount(expectedEventsInEventGroup);
                eventGroup.UpdateEvents.ElementAt(expectedEventsInEventGroup - 1).Should().BeOfType(eventType);
            }
        }
    }
}
