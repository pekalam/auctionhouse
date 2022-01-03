using Auctions.Domain;
using Core.DomainFramework;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Auctions.DomainEvents.Events.V1;

namespace Test.Auctions.Domain
{
    public class Auction_BuyTests
    {
        [Fact]
        public async Task Cannot_be_bought_by_owner()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyerId = auction.Owner;
            var auctionPaymentVerificationScenario = AuctionPaymentVerificationContracts.ValidParams(auction, buyerId);
            var auctionPaymentVerification = new GivenAuctionPaymentVerification().Create(auctionPaymentVerificationScenario);

            var paymentMethod = auctionPaymentVerificationScenario.Given.paymentMethod;
            await Assert.ThrowsAsync<DomainException>(() => auction.Buy(buyerId, paymentMethod, auctionPaymentVerification));
        }

        private async Task<(Auction auction, AuctionPaymentVerificationScenario scenario, UserId buyerId)> CreateAndBuyAuction()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyerId = UserId.New();
            var auctionPaymentVerificationScenario = AuctionPaymentVerificationContracts.ValidParams(auction, buyerId);
            var auctionPaymentVerification = new GivenAuctionPaymentVerification().Create(auctionPaymentVerificationScenario);
            auction.MarkPendingEventsAsHandled();
            await auction.Buy(buyerId, auctionPaymentVerificationScenario.Given.paymentMethod, auctionPaymentVerification);
            return (auction, auctionPaymentVerificationScenario, buyerId);
        }

        [Fact]
        public async Task Can_be_bought_and_emits_tx_started_event()
        {
            var (auction, auctionPaymentVerificationScenario, buyerId) = await CreateAndBuyAuction();

            auction.PendingEvents.Count.Should().Be(1);
            var txStartedEvent = auction.PendingEvents.First() as BuyNowTXStarted;
            txStartedEvent.Should().NotBeNull();
            auction.LockIssuer.Should().Be(buyerId);
            txStartedEvent.AuctionId.Should().Be(auction.AggregateId);
            txStartedEvent.BuyerId.Should().Be(buyerId);
            txStartedEvent.Price.Should().Be(auction.BuyNowPrice);
            txStartedEvent.PaymentMethod.Should().Be(auctionPaymentVerificationScenario.Given.paymentMethod);
            txStartedEvent.TransactionId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Can_confirm_buy_and_emits_tx_success()
        {
            var (auction, auctionPaymentVerificationScenario, buyerId) = await CreateAndBuyAuction();
            var txStartedEvent = auction.PendingEvents.First() as BuyNowTXStarted;

            auction.MarkPendingEventsAsHandled();
            auction.ConfirmBuy(txStartedEvent.TransactionId).Should().BeTrue();

            auction.PendingEvents.Count.Should().Be(1);
            auction.PendingEvents.First().Should().BeOfType<BuyNowTXSuccess>();
            auction.Completed.Should().BeTrue();
        }

        [Fact]
        public async Task Cannot_confirm_with_invalid_tx_id_and_emits_failed_event()
        {
            var (auction, auctionPaymentVerificationScenario, buyerId) = await CreateAndBuyAuction();

            auction.MarkPendingEventsAsHandled();
            auction.ConfirmBuy(Guid.NewGuid()).Should().BeFalse();

            auction.PendingEvents.Count.Should().Be(1);
            auction.PendingEvents.First().Should().BeOfType<BuyNowTXFailed>();
            auction.Completed.Should().BeFalse();
        }
    }
}
