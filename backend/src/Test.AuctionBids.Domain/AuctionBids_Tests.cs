using System;
using Xunit;
using FluentAssertions;

namespace Test.AuctionBids.Domain
{
    using Core.Common.Domain.AuctionBids;
    using System.Linq;

    public class AuctionBids_Tests
    {
        private AuctionBids auctionBids = GivenValidAuctionBids();

        private static AuctionBids GivenValidAuctionBids()
        {
            return AuctionBids.CreateNew(GivenValidAuctionId(), GivenValidUserId());
        }

        private static AuctionId GivenValidAuctionId()
        {
            return AuctionId.New();
        }

        private static UserId GivenValidUserId()
        {
            return UserId.New();
        }

        [Fact]
        public void Creates_event_indicating_aggregate_creation()
        {
            auctionBids.PendingEvents.Count.Should().Be(1);
            var createdEvent = auctionBids.PendingEvents.First() as Events.V1.AuctionBidsCreated;

            createdEvent.Should().NotBeNull();
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
            var raisedEvent = auctionBids.PendingEvents.First() as AuctionPriceRised;

            raisedEvent.WinnerId.Should().Be(newWinner);
            raisedEvent.CurrentPrice.Should().Be(1m);
        }

        [Fact]
        public void Cannot_be_raised_by_owner()
        {
            Assert.Throws<InvalidUserIdException>(() => auctionBids.TryRaise(auctionBids.OwnerId, 1m));
        }

        [Fact]
        public void Can_cancel_bid()
        {
            auctionBids.MarkPendingEventsAsHandled();
            var newWinner = GivenValidUserId();
            var bid = GivenAcceptedBid(newWinner);
            auctionBids.MarkPendingEventsAsHandled();

            auctionBids.CancelBid(bid.Id);

            auctionBids.PendingEvents.Count().Should().Be(2);
            bid.Cancelled.Should().BeTrue();
            auctionBids.PendingEvents.First().Should().BeOfType<BidCancelled>();
            auctionBids.PendingEvents.ElementAt(1).Should().BeOfType<AuctionHaveNoParticipants>();
            auctionBids.WinnerId.Should().BeNull();
            auctionBids.CurrentPrice.Should().Be(default(decimal));
        }

        private Bid GivenAcceptedBid(UserId newWinner)
        {
            var bid = auctionBids.TryRaise(newWinner, auctionBids.CurrentPrice + 1m);
            auctionBids.CurrentPrice.Should().Be(bid.Price);
            return bid;
        }

        private Bid GivenNotAcceptedBid(UserId newWinner)
        {
            var bid = auctionBids.TryRaise(newWinner, auctionBids.CurrentPrice);
            auctionBids.CurrentPrice.Should().Be(bid.Price);
            return bid;
        }

        [Fact]
        public void Subsequent_bids_raise_price()
        {
            auctionBids.MarkPendingEventsAsHandled();
            for (int i = 0; i < 10; i++)
            {
                var newWinner = GivenValidUserId();
                var lastBid = auctionBids.TryRaise(newWinner, auctionBids.CurrentPrice + 1m);
                auctionBids.PendingEvents.Count.Should().Be(1);
                auctionBids.PendingEvents.First().Should().BeOfType<AuctionPriceRised>();
                auctionBids.CurrentPrice.Should().Be(lastBid.Price);
                auctionBids.MarkPendingEventsAsHandled();
            }
        }

        [Fact]
        public void Cannot_raise_price_if_price_less_than_current_and_emits_notaccepted_bid_and_event()
        {
            auctionBids.MarkPendingEventsAsHandled();
            var newWinner = GivenValidUserId();
            Bid bid;
            var firstBid = GivenAcceptedBid(newWinner);
            auctionBids.MarkPendingEventsAsHandled();
            newWinner = GivenValidUserId();
            bid = GivenNotAcceptedBid(newWinner);


            auctionBids.Bids.Count().Should().Be(2);
            auctionBids.PendingEvents.Count().Should().Be(1);
            auctionBids.PendingEvents.ElementAt(0).Should().BeOfType<AuctionBidNotAccepted>();
            var notAcceptedEvent = auctionBids.PendingEvents.First() as AuctionBidNotAccepted;
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
            var newWinner = GivenValidUserId();
            _ = GivenAcceptedBid(newWinner);
            _ = GivenAcceptedBid(newWinner);

            var recreatedBids = AuctionBids.FromEvents(auctionBids.PendingEvents);
            recreatedBids.PendingEvents.Count().Should().Be(0);

            auctionBids.MarkPendingEventsAsHandled();
            auctionBids.Should().BeEquivalentTo(recreatedBids);
        }
    }
}
