using Auctions.Domain.Tests.Assertions;
using Auctions.DomainEvents;
using Auctions.Tests.Base.Domain.ModelBuilders.Shared;
using Auctions.Tests.Base.Domain.Services.Fakes;
using Auctions.Tests.Base.Domain.Services.ServiceContracts;
using Core.Common.Domain;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Auctions.DomainEvents.Events.V1;

namespace Auctions.Domain.Tests
{
    public class Auction_BuyNowTxEvents_Tests : Auction_Buy_Tests
    {
        [Fact]
        public async Task Emits_TXEStartedvent_And_AuctionLocked_when_bought_by_a_non_onwer_with_valid_payment_method()
        {
            var auction = GivenValidAuctionThatCanBeBought();
            var buyerId = GivenUserId.Build();
            var successPaymentVerificationScenario = AuctionPaymentVerificationContracts.SuccessfulScenario(auction, buyerId);

            await BuyAuction(auction, buyerId, successPaymentVerificationScenario);

            ShouldBeValidAuctionLockedEvent(auction.PendingEvents.First(), auction);
            ShouldBeValidTxStartedEventForAuction(auction.PendingEvents.Skip(1).First(), auction, buyerId, successPaymentVerificationScenario);
            auction.PendingEvents.Should().HaveCount(2);
        }

        private static void ShouldBeValidTxStartedEventForAuction(Event @event, Auction auction, UserId buyerId, AuctionPaymentVerificationScenario auctionPaymentVerificationScenario)
        {
            @event.Should().BeOfType<BuyNowTXStarted>();
            var txStartedEvent = (BuyNowTXStarted)auction.PendingEvents.First(t => t.GetType() == typeof(BuyNowTXStarted));
            txStartedEvent.Should().NotBeNull();
            txStartedEvent.AuctionId.Should().Be(auction.AggregateId);
            txStartedEvent.BuyerId.Should().Be(buyerId);
            txStartedEvent.Price.Should().Be(auction.BuyNowPrice);
            txStartedEvent.PaymentMethodName.Should().Be(auctionPaymentVerificationScenario.Given.paymentMethod);
            txStartedEvent.TransactionId.Should().Be(auction.TransactionId.Value);
        }

        private static void ShouldBeValidAuctionLockedEvent(Event @event, Auction auction)
        {
            var auctionLockedEvent = (AuctionLocked)@event;
            auctionLockedEvent.AuctionId.Should().Be(auction.AggregateId.Value);
            auctionLockedEvent.LockIssuer.Should().Be(auction.LockIssuer.Value);
        }

        [Fact]
        public async Task Emits_TXSuccessEvent_and_AuctionUnlocked_when_confirmed_buy_for_a_started_transaction()
        {
            var buyerId = GivenUserId.Build();
            var (auction, transactionId) = await GivenBoughtAuction(buyerId);

            auction.ConfirmBuy(transactionId, FakeAuctionUnlockScheduler.Instance);

            ShouldBeValidAuctionUnlockedEvent(auction.PendingEvents.First(), auction);
            ShouldBeValidTxSuccessEventForAuction(auction.PendingEvents.Skip(1).First(), transactionId, auction, buyerId);
            auction.PendingEvents.Should().HaveCount(2);
        }

        private static void ShouldBeValidTxSuccessEventForAuction(Event @event, Guid transactionId, Auction auction, UserId buyerId)
        {
            var txSuccessEvent = (BuyNowTXSuccess)@event;
            txSuccessEvent.BuyerId.Should().Be(buyerId);
            txSuccessEvent.TransactionId.Should().Be(transactionId);
            txSuccessEvent.AuctionId.Should().Be(auction.AggregateId);
        }

        private static void ShouldBeValidAuctionUnlockedEvent(Event @event, Auction auction)
        {
            var auctionLockedEvent = (AuctionUnlocked)@event;
            auctionLockedEvent.AuctionId.Should().Be(auction.AggregateId.Value);
        }

        [Fact]
        public async Task Emits_TXFailed_Event_when_confirms_buy_for_invalid_transaction()
        {
            var buyerId = GivenUserId.Build();
            var (auction, _) = await GivenBoughtAuction(buyerId);
            var nonExistingTransactionId = Guid.NewGuid();

            auction.ConfirmBuy(nonExistingTransactionId, FakeAuctionUnlockScheduler.Instance);

            var @event = auction.ShouldEmitSingleEvent();
            ShouldBeValidTxFailedEventForAuction(@event, nonExistingTransactionId, auction);
        }

        [Fact]
        public async Task Emits_TXFailed_Event_when_confirms_buy_after_another_transaction_is_started()
        {
            var buyerId = GivenUserId.Build();
            var (auction, transactionId) = await GivenBoughtAuction(buyerId);
            // scheduled unlock happens - now anyone can start buy now tx
            auction.Unlock(auction.LockIssuer);
            var secondBuyerId = GivenUserId.Build();
            await BuyAuction(auction, secondBuyerId);
            auction.MarkPendingEventsAsHandled();

            auction.ConfirmBuy(transactionId, FakeAuctionUnlockScheduler.Instance);

            var @event = auction.ShouldEmitSingleEvent();
            ShouldBeValidTxFailedEventForAuction(@event, transactionId, auction);
        }

        private static void ShouldBeValidTxFailedEventForAuction(Event @event, Guid transactionId, Auction auction)
        {
            var txFailedEvent = (BuyNowTXFailed)@event;
            txFailedEvent.TransactionId.Should().Be(transactionId);
            txFailedEvent.AuctionId.Should().Be(auction.AggregateId);
        }

        [Fact]
        public async Task Emits_TXCanceled_and_AuctionUnlocked_events_when_cancelled_with_valid_transaction_id()
        {
            var buyerId = GivenUserId.Build();
            var (auction, transactionId) = await GivenBoughtAuction(buyerId);

            auction.CancelBuy(transactionId, FakeAuctionUnlockScheduler.Instance);

            ShouldBeValidAuctionUnlockedEvent(auction.PendingEvents.First(), auction);
            ShouldBeValidTXCancelledEventForAuction(auction.PendingEvents.Skip(1).First(), transactionId, auction);
            auction.PendingEvents.Should().HaveCount(2);
        }

        private void ShouldBeValidTXCancelledEventForAuction(Event @event, Guid transactionId, Auction auction)
        {
            var txCancelled = (BuyNowTXCanceled)@event;

            txCancelled.TransactionId.Should().Be(transactionId);
            txCancelled.AuctionId.Should().Be(auction.AggregateId);
        }

        [Fact]
        public async Task Emits_TXCancelledConcurrently_Event_when_cancelled_after_new_transaction_is_created()
        {
            //Arrange
            var buyerId = GivenUserId.Build();
            var (auction, transactionId) = await GivenBoughtAuction(buyerId);
            // scheduled unlock happens - now anyone can start buy now tx
            auction.Unlock(auction.LockIssuer);

            // creates new transaction
            await BuyAuction(auction);
            auction.MarkPendingEventsAsHandled();

            //Act
            auction.CancelBuy(transactionId, FakeAuctionUnlockScheduler.Instance);

            //Assert
            var @event = auction.ShouldEmitSingleEvent();
            ShouldBeValidTxCancelledConcurrentlyEventForAuction(@event, transactionId, auction);
        }

        private void ShouldBeValidTxCancelledConcurrentlyEventForAuction(Event @event, Guid transactionId, Auction auction)
        {
            var txCancelledConcurrently = (BuyNowTXCanceledConcurrently)@event;

            txCancelledConcurrently.TransactionId.Should().Be(transactionId);
            txCancelledConcurrently.AuctionId.Should().Be(auction.AggregateId);
        }

        //TODO scenario1 / scenario2 refactor

        [Fact]
        public async Task Can_be_bought_in_concurrent_scenario1()
        {
            var (auction, _) = await GivenBoughtAuction();
            var transactionId1 = auction.TransactionId;
            // scheduled unlock happens - anyone can start buy now tx
            auction.Unlock(auction.LockIssuer);

            // user2 buys auction
            var user2 = GivenUserId.Build();
            var transactionId2 = await BuyAuction(auction, user2);
            transactionId1.Value.Should().NotBe(transactionId2);
            auction.TransactionId.Value.Should().NotBe(transactionId1.Value);

            // user2 confirms
            auction.ConfirmBuy(transactionId2, FakeAuctionUnlockScheduler.Instance);
            auction.Completed.Should().BeTrue();
            auction.Locked.Should().BeFalse();
            auction.Buyer.Value.Should().Be(user2.Value);
            auction.MarkPendingEventsAsHandled();

            // user1 cancels
            auction.CancelBuy(transactionId1.Value, FakeAuctionUnlockScheduler.Instance).Should().BeFalse();
            // still completed
            auction.Completed.Should().BeTrue();
            auction.Locked.Should().BeFalse();
            auction.Buyer.Value.Should().Be(user2.Value);
            AssertBuyNowTxCanceledConcurrentlyEvent(auction, transactionId1);
        }

        [Fact]
        public async Task Can_be_bought_in_concurrent_scenario2()
        {
            var (auction, _) = await GivenBoughtAuction();
            var transactionId1 = auction.TransactionId;
            // scheduled unlock happens - anyone can start buy now tx
            auction.Unlock(auction.LockIssuer);

            // user2 buys auction
            var user2 = GivenUserId.Build();
            var transactionId2 = await BuyAuction(auction, user2);
            transactionId1.Value.Should().NotBe(transactionId2);
            auction.TransactionId.Value.Should().NotBe(transactionId1.Value);
            auction.MarkPendingEventsAsHandled();

            // user1 cancels
            auction.CancelBuy(transactionId1.Value, FakeAuctionUnlockScheduler.Instance).Should().BeFalse();
            auction.Completed.Should().BeFalse();
            // still locked by user2
            auction.Locked.Should().BeTrue();
            AssertBuyNowTxCanceledConcurrentlyEvent(auction, transactionId1);
            auction.MarkPendingEventsAsHandled();

            // user2 confirms
            auction.ConfirmBuy(transactionId2, FakeAuctionUnlockScheduler.Instance);
            auction.Completed.Should().BeTrue();
            auction.Locked.Should().BeFalse();
            auction.Buyer.Value.Should().Be(user2.Value);
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
