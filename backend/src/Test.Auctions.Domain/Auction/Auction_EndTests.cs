using Auctions.Domain.Services;
using Auctions.Domain.Tests.Assertions;
using Auctions.DomainEvents;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Auctions.Tests.Base.Domain.ModelBuilders.Shared;
using Auctions.Tests.Base.Domain.Services.TestDoubleBuilders;
using Core.Common.Domain;
using Core.DomainFramework;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Auctions.Domain.Tests
{
    public class Auction_End_Tests
    {
        [Fact]
        public void New_auction_can_be_successfuly_ended()
        {
            var auction = GivenValidAuction();

            auction.EndAuction();

            AuctionStateShouldBeValidAfterEnding(auction);
            var @event = auction.ShouldEmitSingleEvent();
            AuctionEndedEventShouldBeValid(@event, auction);
        }

        private static Auction GivenValidAuction()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            auction.MarkPendingEventsAsHandled();
            return auction;
        }

        private void AuctionEndedEventShouldBeValid(Event @event, Auction auction)
        {
            var auctionEnded = (AuctionEnded)@event;
            auctionEnded.AuctionId.Should().Be(auction.AggregateId);
        }

        private void AuctionStateShouldBeValidAfterEnding(Auction auction)
        {
            auction.Completed.Should().BeTrue();
        }

        [Fact]
        public void Fails_when_trying_to_end_while_locked()
        {
            var auction = GivenLockedAuction();
            var endAuctionAction = () => auction.EndAuction();

            endAuctionAction.Should().Throw<DomainException>().WithMessage("Cannot end locked auction");
        }

        private static Auction GivenLockedAuction()
        {
            var auction = GivenValidAuction();
            auction.Lock(GivenUserId.Build());
            return auction;
        }

        [Fact]
        public async Task Fails_when_trying_to_end_while_completed()
        {
            var auction = await GivenCompletedAuction();
            var endAuctionAction = () => auction.EndAuction();

            endAuctionAction.Should().Throw<DomainException>().WithMessage("Cannot end completed auction");
        }

        private static async Task<Auction> GivenCompletedAuction()
        {
            var auction = GivenValidAuction();
            var buyer = GivenUserId.Build();
            var tranId = await auction.Buy(buyer, "test", new GivenAuctionPaymentVerification().BuildValid(auction, buyer), Mock.Of<IAuctionUnlockScheduler>());
            auction.ConfirmBuy(tranId, Mock.Of<IAuctionUnlockScheduler>());
            return auction;
        }
    }
}
