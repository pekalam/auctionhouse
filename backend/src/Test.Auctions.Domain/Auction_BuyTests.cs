using Auctions.Domain;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Auctions.Tests.Base.ServiceContracts;
using Core.DomainFramework;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Auctions.Tests.Base.Builders;
using Xunit;
using static Auctions.DomainEvents.Events.V1;

namespace Auctions.Domain.Tests
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
            txStartedEvent.PaymentMethodName.Should().Be(auctionPaymentVerificationScenario.Given.paymentMethod);
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
            auction.Locked.Should().BeFalse();
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

        [Fact]
        public async Task Can_be_bought_in_concurrent_scenario1()
        {
            var (auction, _, _) = await CreateAndBuyAuction();
            var transactionId1 = auction.TransactionId;
            // scheduled unlock happens - anyone can start buy now tx
            auction.Unlock(auction.LockIssuer);

            // user2 buys auction
            var user2 = UserId.New();
            var auctionPaymentVerification = CreateAuctionPaymentVerification(auction, user2);
            await auction.Buy(user2, "test", auctionPaymentVerification, Mock.Of<IAuctionUnlockScheduler>());
            var transactionId2 = auction.TransactionId;
            transactionId1.Value.Should().NotBe(transactionId2.Value);
            auction.TransactionId.Value.Should().NotBe(transactionId1.Value);

            // user2 confirms
            auction.ConfirmBuy(transactionId2.Value, Mock.Of<IAuctionUnlockScheduler>());
            auction.Completed.Should().BeTrue();
            auction.Locked.Should().BeFalse();
            auction.Buyer.Value.Should().Be(user2.Value);
            auction.MarkPendingEventsAsHandled();

            // user1 cancels
            auction.CancelBuy(transactionId1.Value, Mock.Of<IAuctionUnlockScheduler>()).Should().BeFalse();
            // still completed
            auction.Completed.Should().BeTrue();
            auction.Locked.Should().BeFalse();
            auction.Buyer.Value.Should().Be(user2.Value);
            AssertBuyNowTxCanceledConcurrentlyEvent(auction, transactionId1);
        }

        [Fact]
        public async Task Can_be_bought_in_concurrent_scenario2()
        {
            var (auction, _, _) = await CreateAndBuyAuction();
            var transactionId1 = auction.TransactionId;
            // scheduled unlock happens - anyone can start buy now tx
            auction.Unlock(auction.LockIssuer);

            // user2 buys auction
            var user2 = UserId.New();
            var auctionPaymentVerification = CreateAuctionPaymentVerification(auction, user2);
            await auction.Buy(user2, "test", auctionPaymentVerification, Mock.Of<IAuctionUnlockScheduler>());
            var transactionId2 = auction.TransactionId;
            transactionId1.Value.Should().NotBe(transactionId2.Value);
            auction.TransactionId.Value.Should().NotBe(transactionId1.Value);
            auction.MarkPendingEventsAsHandled();

            // user1 cancels
            auction.CancelBuy(transactionId1.Value, Mock.Of<IAuctionUnlockScheduler>()).Should().BeFalse();
            auction.Completed.Should().BeFalse();
            // still locked by user2
            auction.Locked.Should().BeTrue();
            AssertBuyNowTxCanceledConcurrentlyEvent(auction, transactionId1);
            auction.MarkPendingEventsAsHandled();

            // user2 confirms
            auction.ConfirmBuy(transactionId2.Value, Mock.Of<IAuctionUnlockScheduler>());
            auction.Completed.Should().BeTrue();
            auction.Locked.Should().BeFalse();
            auction.Buyer.Value.Should().Be(user2.Value);
        }

        [Fact]
        public async Task Can_be_cancelled_by_user_that_started_buynowtx()
        {
            var (auction, _, _) = await CreateAndBuyAuction();
            auction.MarkPendingEventsAsHandled();
            var transactionId = auction.TransactionId;
            auction.CancelBuy(auction.TransactionId.Value, Mock.Of<IAuctionUnlockScheduler>());
            AssertBuyNowTxCanceledEvent(auction, transactionId);
        }

        [Fact]
        public async Task Can_be_cancelled_by_user_that_started_buynowtx_and_auction_was_unlocked()
        {
            var (auction, _, _) = await CreateAndBuyAuction();
            auction.Unlock(auction.LockIssuer);
            auction.MarkPendingEventsAsHandled();
            var transactionId = auction.TransactionId;
            auction.CancelBuy(auction.TransactionId.Value, Mock.Of<IAuctionUnlockScheduler>());
            AssertBuyNowTxCanceledEventWithoutUnlock(auction, transactionId);
        }

        [Fact]
        public async Task Emits_TxFailed_event_when_confirm_buy_occurs_and_new_tx_is_started()
        {
            var (auction, _, _) = await CreateAndBuyAuction();
            var transactionId1 = auction.TransactionId;
            // scheduled unlock happens - anyone can start buy now tx
            auction.Unlock(auction.LockIssuer);

            // user2 buys auction
            var user2 = UserId.New();
            var auctionPaymentVerification = CreateAuctionPaymentVerification(auction, user2);
            await auction.Buy(user2, "test", auctionPaymentVerification, Mock.Of<IAuctionUnlockScheduler>());
            var transactionId2 = auction.TransactionId;

            auction.MarkPendingEventsAsHandled();
            auction.ConfirmBuy(transactionId1.Value, Mock.Of<IAuctionUnlockScheduler>()).Should().BeFalse();

            auction.PendingEvents.Count.Should().Be(1);
            auction.PendingEvents.First().Should().BeOfType<BuyNowTXFailed>();
            var failedEvent = auction.PendingEvents.First() as BuyNowTXFailed;
            failedEvent.TransactionId.Should().Be(transactionId1.Value);
        }

        private static IAuctionPaymentVerification CreateAuctionPaymentVerification(Auction auction, UserId user2)
        {
            var auctionPaymentVerificationScenario = AuctionPaymentVerificationContracts.ValidParams(auction, user2);
            var auctionPaymentVerification = new GivenAuctionPaymentVerification().Create(auctionPaymentVerificationScenario);
            return auctionPaymentVerification;
        }

        private static void AssertBuyNowTxCanceledEvent(Auction auction, Guid? transactionId)
        {
            auction.PendingEvents.Count.Should().Be(2);
            auction.PendingEvents.First().Should().BeOfType<AuctionUnlocked>();
            auction.PendingEvents.ElementAt(1).Should().BeOfType<BuyNowTXCanceled>();
            var canceledEvent = auction.PendingEvents.ElementAt(1) as BuyNowTXCanceled;
            canceledEvent.TransactionId.Should().Be(transactionId.Value);
        }

        private static void AssertBuyNowTxCanceledEventWithoutUnlock(Auction auction, Guid? transactionId)
        {
            auction.PendingEvents.Count.Should().Be(1);
            auction.PendingEvents.First().Should().BeOfType<BuyNowTXCanceled>();
            var canceledEvent = auction.PendingEvents.First() as BuyNowTXCanceled;
            canceledEvent.TransactionId.Should().Be(transactionId.Value);
        }


        private static void AssertBuyNowTxCanceledConcurrentlyEvent(Auction auction, Guid? transactionId2)
        {
            auction.PendingEvents.Count.Should().Be(1);
            auction.PendingEvents.First().Should().BeOfType<BuyNowTXCanceledConcurrently>();
            auction.TransactionId.Value.Should().NotBe(transactionId2.Value);
            auction.TransactionId.Value.Should().NotBe(Guid.Empty);
            var canceledEvent = auction.PendingEvents.First() as BuyNowTXCanceledConcurrently;
            canceledEvent.TransactionId.Should().Be(transactionId2.Value);
        }
    }
}
