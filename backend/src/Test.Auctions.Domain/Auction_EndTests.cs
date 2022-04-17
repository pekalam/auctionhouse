using Auctions.Domain;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Core.DomainFramework;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auctions.Tests.Base.Builders;
using Xunit;

namespace Auctions.Domain.Tests
{
    public class Auction_EndTests
    {
        [Fact]
        public void Can_be_ended_when_not_completed()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            auction.MarkPendingEventsAsHandled();

            auction.EndAuction();

            auction.Completed.Should().BeTrue();
            auction.PendingEvents.Count.Should().Be(1);
            auction.PendingEvents.First().Should().BeOfType<AuctionEnded>();
            ((AuctionEnded)auction.PendingEvents.First()).AuctionId.Should().Be(auction.AggregateId.Value);
        }

        [Fact]
        public void Cannot_be_ended_while_locked()
        {
            var auction = new GivenAuction().WithAssignedAuctionBidsId(null).Build();

            Assert.Throws<DomainException>(() => auction.EndAuction());
        }

        [Fact]
        public async Task Cannot_be_ended_when_completedAsync()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyer = UserId.New();
            var tranId = await auction.Buy(buyer, "test", new GivenAuctionPaymentVerification().CreateValid(auction, buyer), Mock.Of<IAuctionUnlockScheduler>());
            auction.ConfirmBuy(tranId, Mock.Of<IAuctionUnlockScheduler>());

            Assert.Throws<DomainException>(() => auction.EndAuction());
        }
    }
}
