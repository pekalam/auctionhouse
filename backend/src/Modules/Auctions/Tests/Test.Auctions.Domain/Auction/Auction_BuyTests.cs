using Auctions.Domain.Services;
using Auctions.Domain.Tests.Assertions;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Auctions.Tests.Base.Domain.ModelBuilders.Shared;
using Auctions.Tests.Base.Domain.Services.Fakes;
using Auctions.Tests.Base.Domain.Services.ServiceContracts;
using Auctions.Tests.Base.Domain.Services.TestDoubleBuilders;
using Core.DomainFramework;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Auctions.Domain.Tests
{
    [Trait("Category", "Unit")]
    public class Auction_Buy_Tests
    {
        [Fact]
        public async Task Fails_when_bought_by_the_owner()
        {
            var auction = GivenValidAuctionThatCanBeBought();

            var buyAuctionAction = () => BuyAuction(auction, auction.Owner);

            var exception = await buyAuctionAction
                .Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Cannot be bought by owner");
            auction.ShouldEmitNoEvents();
        }

        [Fact]
        public async Task Succeeds_when_bought_by_a_non_onwer_with_valid_payment_method()
        {
            var auction = GivenValidAuctionThatCanBeBought();
            var buyerId = GivenUserId.Build();

            var transactionId = await BuyAuction(auction, buyerId);

            transactionId.Should().NotBe(Guid.Empty);
            AuctionStateShouldBeValidAfterBeingBought(auction, buyerId, transactionId);
        }

        [Fact]
        public async Task Succeeds_when_confirms_buy_for_a_started_transaction()
        {
            var buyerId = GivenUserId.Build();
            var (auction, transactionId) = await GivenBoughtAuction(buyerId);

            var confirmBuyResult = auction.ConfirmBuy(transactionId, FakeAuctionUnlockScheduler.Instance);

            confirmBuyResult.Should().BeTrue();
            AuctionStateShouldBeValidAfterBuyConfirmed(auction);
        }

        [Fact]
        public async Task Fails_when_confirms_buy_for_non_existing_transaction_id()
        {
            var (auction, _) = await GivenBoughtAuction();
            var nonExistingTransactionId = Guid.NewGuid();

            var confirmBuyResult = auction.ConfirmBuy(nonExistingTransactionId, FakeAuctionUnlockScheduler.Instance);

            confirmBuyResult.Should().BeFalse();
            AuctionStateShouldBeValidAfterBuyConfirmationFails(auction);
        }

        [Fact]
        public async Task Succeeds_when_cancelled_with_valid_transaction_id()
        {
            var (auction, transactionId) = await GivenBoughtAuction();

            var cancelBuyResult = auction.CancelBuy(transactionId, FakeAuctionUnlockScheduler.Instance);

            cancelBuyResult.Should().BeTrue();
            AuctionStateShoudBeValidAfterBuyCanceled(auction);
        }

        [Fact]
        public async Task Fails_when_cancelled_with_non_existing_transaction_id()
        {
            var (auction, _) = await GivenBoughtAuction();
            var nonExistingTransactionId = Guid.NewGuid();

            var cancelBuyResult = auction.CancelBuy(nonExistingTransactionId, FakeAuctionUnlockScheduler.Instance);

            cancelBuyResult.Should().BeFalse();
            AuctionStateShoudBeValidAfterBuyCanceledFails(auction);
        }

        [Fact]
        public async Task Fails_when_cancelled_after_new_transaction_is_created()
        {
            // Arrange
            var (auction, transactionId) = await GivenBoughtAuction();
            var secondBuyerId = GivenUserId.Build();

            //Act
            auction.Unlock(auction.LockIssuer);
            // scheduled unlock happens - now anyone can start buy now tx
            var secondTransactionId = await BuyAuction(auction, secondBuyerId);
            var cancelBuyResult = auction.CancelBuy(transactionId, FakeAuctionUnlockScheduler.Instance);

            //Assert
            cancelBuyResult.Should().BeFalse();
            AuctionStateShouldBeValidAfterBeingBought(auction, secondBuyerId, secondTransactionId);
        }

        protected async Task<(Auction Auction, Guid TransactionId)> GivenBoughtAuction(UserId buyerId = null)
        {
            var auction = GivenValidAuctionThatCanBeBought();
            var transactionId = await BuyAuction(auction, buyerId ?? GivenUserId.Build());
            auction.MarkPendingEventsAsHandled();
            return (auction, transactionId);
        }

        protected Auction GivenValidAuctionThatCanBeBought()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            auction.MarkPendingEventsAsHandled();
            return auction;
        }

        protected Task<Guid> BuyAuction(Auction auction, UserId buyerId = null, AuctionPaymentVerificationScenario paymentVerificationScenario = null)
        {
            buyerId ??= GivenUserId.Build();
            paymentVerificationScenario ??= AuctionPaymentVerificationContracts.SuccessfulScenario(auction, buyerId);
            var stubPaymentVerification = new GivenAuctionPaymentVerification().Build(paymentVerificationScenario);
            return auction.Buy(buyerId, paymentVerificationScenario.Given.paymentMethod, stubPaymentVerification, Mock.Of<IAuctionUnlockScheduler>());
        }

        private void AuctionStateShoudBeValidAfterBuyCanceledFails(Auction auction)
        {
            auction.Locked.Should().BeTrue();
            auction.Completed.Should().BeFalse();
        }

        private void AuctionStateShoudBeValidAfterBuyCanceled(Auction auction)
        {
            auction.Locked.Should().BeFalse();
            auction.Completed.Should().BeFalse();
        }

        private static void AuctionStateShouldBeValidAfterBuyConfirmationFails(Auction auction)
        {
            auction.TransactionId.HasValue.Should().BeFalse();
            auction.Completed.Should().BeFalse();
            auction.Locked.Should().BeTrue();
        }

        private static void AuctionStateShouldBeValidAfterBuyConfirmed(Auction auction)
        {
            auction.TransactionId.HasValue.Should().BeTrue();
            auction.TransactionId.Should().NotBe(Guid.Empty);
            auction.Completed.Should().BeTrue();
            auction.Locked.Should().BeFalse();
        }

        private static void AuctionStateShouldBeValidAfterBeingBought(Auction auction, UserId buyerId, Guid transactionId)
        {
            auction.TransactionId.HasValue.Should().BeTrue();
            auction.TransactionId.Should().NotBe(Guid.Empty);
            auction.TransactionId.Should().Be(transactionId);
            auction.LockIssuer.Should().Be(buyerId);
            auction.Completed.Should().BeFalse();
            auction.Locked.Should().BeTrue();
        }
    }
}
