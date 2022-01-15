using Auctions.Domain;
using Auctions.Domain.Services;
using Core.DomainFramework;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Test.Auctions.Base.Builders;
using Test.Auctions.Base.ServiceContracts;
using Xunit;
using static Auctions.DomainEvents.Events.V1;

namespace Test.AuctionsDomain
{
    [Trait("Category", "Unit")]
    public class Auction_BuyTests
    {
        private async Task<(Auction auction, AuctionPaymentVerificationScenario scenario, UserId buyerId)> CreateAndBuyAuction()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyerId = UserId.New();
            var auctionPaymentVerificationScenario = AuctionPaymentVerificationContracts.ValidParams(auction, buyerId);
            var auctionPaymentVerification = new GivenAuctionPaymentVerification().Create(auctionPaymentVerificationScenario);
            auction.MarkPendingEventsAsHandled();
            await auction.Buy(buyerId, auctionPaymentVerificationScenario.Given.paymentMethod, auctionPaymentVerification, Mock.Of<IAuctionUnlockScheduler>());
            return (auction, auctionPaymentVerificationScenario, buyerId);
        }

        [Fact]
        public async Task Cannot_be_bought_by_owner()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyerId = auction.Owner;
            var auctionPaymentVerificationScenario = AuctionPaymentVerificationContracts.ValidParams(auction, buyerId);
            var auctionPaymentVerification = new GivenAuctionPaymentVerification().Create(auctionPaymentVerificationScenario);

            var paymentMethod = auctionPaymentVerificationScenario.Given.paymentMethod;
            await Assert.ThrowsAsync<DomainException>(() => auction.Buy(buyerId, paymentMethod, auctionPaymentVerification, Mock.Of<IAuctionUnlockScheduler>()));
        }

        [Fact]
        public async Task Can_be_bought_and_emits_tx_started_event()
        {
            var (auction, auctionPaymentVerificationScenario, buyerId) = await CreateAndBuyAuction();

            auction.TransactionId.HasValue.Should().BeTrue();
            auction.TransactionId.Should().NotBe(Guid.Empty);
            auction.LockIssuer.Should().Be(buyerId);
            var txStartedEvent = (BuyNowTXStarted)auction.PendingEvents.First(t => t.GetType() == typeof(BuyNowTXStarted));
            txStartedEvent.Should().NotBeNull();
            txStartedEvent.AuctionId.Should().Be(auction.AggregateId);
            txStartedEvent.BuyerId.Should().Be(buyerId);
            txStartedEvent.Price.Should().Be(auction.BuyNowPrice);
            txStartedEvent.PaymentMethod.Should().Be(auctionPaymentVerificationScenario.Given.paymentMethod);
            txStartedEvent.TransactionId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Can_confirm_buy_and_emits_tx_success()
        {
            var (auction, _, _) = await CreateAndBuyAuction();
            var txStartedEvent = (BuyNowTXStarted)auction.PendingEvents.First(t => t.GetType() == typeof(BuyNowTXStarted));
            auction.TransactionId.HasValue.Should().BeTrue();
            auction.TransactionId.Should().NotBe(Guid.Empty);

            auction.MarkPendingEventsAsHandled();
            auction.ConfirmBuy(txStartedEvent.TransactionId, Mock.Of<IAuctionUnlockScheduler>()).Should().BeTrue();
            auction.TransactionId.HasValue.Should().BeTrue();
            auction.TransactionId.Should().NotBe(Guid.Empty);

            auction.PendingEvents.FirstOrDefault(t => t.GetType() == typeof(BuyNowTXSuccess)).Should().NotBeNull();
            auction.Completed.Should().BeTrue();
        }

        [Fact]
        public async Task Cannot_confirm_with_invalid_tx_id_and_emits_failed_event()
        {
            var (auction, _, _) = await CreateAndBuyAuction();
            auction.TransactionId.HasValue.Should().BeTrue();
            auction.TransactionId.Should().NotBe(Guid.Empty);

            auction.MarkPendingEventsAsHandled();
            auction.ConfirmBuy(Guid.NewGuid(), Mock.Of<IAuctionUnlockScheduler>()).Should().BeFalse();
            auction.TransactionId.HasValue.Should().BeFalse();

            auction.PendingEvents.FirstOrDefault(e => e.GetType() == typeof(BuyNowTXFailed)).Should().NotBeNull();
            auction.Completed.Should().BeFalse();
        }
    }
}
