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
    public class Auction_End_Tests : Auction_Buy_Tests //TODO: temporary
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
        public async Task Fails_when_trying_to_end_bought()
        {
            var (auction, _) = await GivenBoughtAuction();
            var endAuctionAction = () => auction.EndAuction();

            endAuctionAction.Should().Throw<DomainException>().WithMessage("Cannot end bought auction");
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
            var buyerId = GivenUserId.Build();
            await auction.Buy(buyerId, "test", new GivenAuctionPaymentVerification().BuildValid(auction, buyerId), Mock.Of<IAuctionBuyCancellationScheduler>());
            auction.ConfirmBuy(buyerId, Mock.Of<IAuctionBuyCancellationScheduler>());
            return auction;
        }
    }
}
