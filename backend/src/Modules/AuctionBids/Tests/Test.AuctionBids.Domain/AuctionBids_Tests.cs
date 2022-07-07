using System;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Collections;
using Core.Common.Domain;
using System.Collections.Generic;

namespace AuctionBids.Domain.Tests
{
    using Domain;
    using Domain.Shared;
    using DomainEvents;
    using Core.DomainFramework;

    public static class TestUtils
    {
        public static void ShouldContainEvents(this IEnumerable<Event> events, Type[] eventTypes)
        {
            events.Select(e => e.GetType()).OrderBy(t => t.ToString())
                .SequenceEqual(eventTypes.OrderBy(t => t.ToString()))
                .Should().BeTrue();
        }
    }



    [Trait("Category", "Unit")]
    public class AuctionBids_Tests
    {
        private AuctionBids auctionBids = GivenValidAuctionBids();

        private static AuctionBids GivenValidAuctionBids()
        {
            return AuctionBids.CreateNew(GivenValidAuctionId(), GivenValidCategoryIds(), GivenValidUserId());
        }

        private static AuctionId GivenValidAuctionId()
        {
            return new AuctionId(Guid.NewGuid());
        }

        private static AuctionCategoryIds GivenValidCategoryIds() => new AuctionCategoryIds(1, 2, 3);

        private static UserId GivenValidUserId()
        {
            return new UserId(Guid.NewGuid());
        }

        [Fact]
        public void Creates_event_indicating_aggregate_creation()
        {
            auctionBids.PendingEvents.Count.Should().Be(1);
            var createdEvent = auctionBids.PendingEvents.First() as Events.V1.AuctionBidsCreated;

            createdEvent.AuctionId.Should().Be(auctionBids.AuctionId);
            createdEvent.CategoryId.Should().Be(auctionBids.AuctionCategoryIds.CategoryId);
            createdEvent.SubCategoryId.Should().Be(auctionBids.AuctionCategoryIds.SubCategoryId);
            createdEvent.SubSubCategoryId.Should().Be(auctionBids.AuctionCategoryIds.SubSubCategoryId);
        }

        [Fact]
        public void Can_raise_auction_price()
        {
            auctionBids.MarkPendingEventsAsHandled();
            var newWinner = GivenValidUserId();

            var bid = auctionBids.TryRaise(newWinner, 1m);

            auctionBids.CurrentPrice.Should().Be(bid.Price);
            auctionBids.WinnerBidId.Should().Be(bid.Id);
            bid.Accepted.Should().BeTrue();
            bid.UserId.Should().Be(newWinner);
            auctionBids.PendingEvents.Count.Should().Be(1);
            var raisedEvent = (AuctionPriceRised)auctionBids.PendingEvents.First(e => e.GetType() == typeof(AuctionPriceRised));
            raisedEvent.WinnerId.Should().Be(newWinner);
            raisedEvent.CurrentPrice.Should().Be(1m);
            raisedEvent.AuctionId.Should().Be(auctionBids.AuctionId);
            raisedEvent.CategoryId.Should().Be(auctionBids.AuctionCategoryIds.CategoryId);
            raisedEvent.SubCategoryId.Should().Be(auctionBids.AuctionCategoryIds.SubCategoryId);
            raisedEvent.SubSubCategoryId.Should().Be(auctionBids.AuctionCategoryIds.SubSubCategoryId);
        }

        [Fact]
        public void Cannot_be_raised_by_owner()
        {
            Assert.Throws<InvalidUserIdException>(() => auctionBids.TryRaise(auctionBids.OwnerId, 1m));
        }

        [Fact]
        public void Can_cancel_bid()
        {
            var currentWinner = auctionBids.WinnerId;
            var newWinner = GivenValidUserId();
            var bid = GivenAcceptedBid(newWinner);

            auctionBids.MarkPendingEventsAsHandled();
            auctionBids.CancelBid(bid.Id);

            bid.Cancelled.Should().BeTrue();
            auctionBids.PendingEvents.ShouldContainEvents(new[] { typeof(BidCancelled), typeof(AuctionHaveNoParticipants) });
            auctionBids.WinnerId.Should().Be(currentWinner);
            auctionBids.CurrentPrice.Should().Be(default);
        }

        private Bid GivenAcceptedBid(UserId newWinner)
        {
            var bid = auctionBids.TryRaise(newWinner, auctionBids.CurrentPrice + 1m);
            auctionBids.CurrentPrice.Should().Be(bid.Price);
            return bid;
        }

        [Fact]
        public void Subsequent_bids_raise_price()
        {
            for (var i = 0; i < 10; i++)
            {
                auctionBids.MarkPendingEventsAsHandled();
                var newWinner = GivenValidUserId();

                var lastBid = auctionBids.TryRaise(newWinner, auctionBids.CurrentPrice + 1m);

                auctionBids.PendingEvents.ShouldContainEvents(new[] { typeof(AuctionPriceRised) });
                auctionBids.CurrentPrice.Should().Be(lastBid.Price);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Cannot_raise_price_if_price_less_or_eq_to_current_and_emits_notaccepted_bid_and_event(bool less)
        {
            var newWinner = GivenValidUserId();
            auctionBids.MarkPendingEventsAsHandled();
            var firstBid = GivenAcceptedBid(newWinner);
            auctionBids.MarkPendingEventsAsHandled();
            newWinner = GivenValidUserId();

            var bid = auctionBids.TryRaise(newWinner, less ? auctionBids.CurrentPrice - 1 : auctionBids.CurrentPrice);

            auctionBids.Bids.Count().Should().Be(2);
            auctionBids.PendingEvents.ShouldContainEvents(new[] { typeof(AuctionBidNotAccepted) });
            var notAcceptedEvent = (AuctionBidNotAccepted)auctionBids.PendingEvents.First(e => e.GetType() == typeof(AuctionBidNotAccepted));
            notAcceptedEvent.Price.Should().Be(bid.Price);
            notAcceptedEvent.UserId.Should().Be(bid.UserId);
            notAcceptedEvent.Date.Should().Be(bid.Date);
            notAcceptedEvent.BidId.Should().Be(bid.Id);
            notAcceptedEvent.AuctionId.Should().Be(auctionBids.AuctionId);
            notAcceptedEvent.AuctionBidsId.Should().Be(auctionBids.AggregateId);
            bid.Accepted.Should().BeFalse();
            auctionBids.WinnerId.Should().Be(firstBid.UserId);
            auctionBids.WinnerBidId.Should().Be(firstBid.Id);
            auctionBids.CurrentPrice.Should().Be(firstBid.Price);
        }

        [Fact]
        public void Recreates_its_state_from_events()
        {
            _ = GivenAcceptedBid(GivenValidUserId());
            _ = GivenAcceptedBid(GivenValidUserId());

            var recreatedBids = AuctionBids.FromEvents(auctionBids.PendingEvents);
            recreatedBids.PendingEvents.Count().Should().Be(0);

            auctionBids.MarkPendingEventsAsHandled();
            auctionBids.Should().BeEquivalentTo(recreatedBids);
        }

        [Fact]
        public void Same_user_cannot_raise_price_if_raised_it_previously()
        {
            var newWinner = GivenValidUserId();
            var bid = GivenAcceptedBid(newWinner);

            Assert.Throws<DomainException>(() => GivenAcceptedBid(newWinner));
        }

        [Fact]
        public void Same_user_that_created_unaccpeted_bid_previously_can_raise_bid_if_didnt_create_previous_accepted()
        {
            var user1 = GivenValidUserId();
            _ = GivenAcceptedBid(user1);
            var user2 = GivenValidUserId();

            var unacceptedBid = auctionBids.TryRaise(user2, auctionBids.CurrentPrice);
            var acceptedBid = auctionBids.TryRaise(user2, auctionBids.CurrentPrice + 1);

            unacceptedBid.Accepted.Should().BeFalse();
            acceptedBid.Accepted.Should().BeTrue();
        }
    }
}
