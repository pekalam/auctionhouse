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
using System.Collections.Generic;
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
            var auction = GivenAuctionThatCanBeBought();

            var buyAuction = () => BuyAuction(auction, auction.Owner);

            var exception = await buyAuction
                .Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Cannot be bought by owner");
            auction.ShouldEmitNoEvents();
        }

        [Fact]
        public async Task Fails_when_buying_already_bought()
        {
            var (auction, _) = await GivenBoughtAuction();
            var buyerId2 = GivenUserId.Build();

            var buyAuction = () => BuyAuction(auction, buyerId2);

            var exception = await buyAuction
                .Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Auction is already bought");
                        auction.ShouldEmitNoEvents();
        }

        [Fact]
        public async Task Fails_when_buying_completed()
        {
            var (auction, _) = await GivenBoughtAuction();
            auction.ConfirmBuy(auction.Buyer, Mock.Of<IAuctionBuyCancellationScheduler>());
            auction.MarkPendingEventsAsHandled();
            var buyerId2 = GivenUserId.Build();

            var buyAuction = () => BuyAuction(auction, buyerId2);

            var exception = await buyAuction
                .Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Auction cannot be bought if it's completed");
            auction.ShouldEmitNoEvents();
        }

        [Fact]
        public async Task Can_be_bought_by_a_non_owner_with_valid_payment_method()
        {
            var auction = GivenAuctionThatCanBeBought();
            var buyerId = GivenUserId.Build();
            var mockAuctionBuyCancellationScheduler = new Mock<IAuctionBuyCancellationScheduler>();

            await BuyAuction(auction, buyerId, mockAuctionCancellationScheduler: mockAuctionBuyCancellationScheduler);

            AuctionStateShouldBeValidAfterBeingBought(auction, buyerId);
            mockAuctionBuyCancellationScheduler.Verify(f => f.ScheduleAuctionBuyCancellation(auction.AggregateId, TimeOnly.FromTimeSpan(TimeSpan.FromMilliseconds(AuctionConstantsFactory.BuyCancellationTime))), Times.Once());
        }

        #region ConfirmBuy

        [Fact]
        public async Task Bought_auction_can_be_confirmed_by_buyer()
        {
            var buyerId = GivenUserId.Build();
            var (auction, _) = await GivenBoughtAuction(buyerId);

            var confirmBuyResult = auction.ConfirmBuy(buyerId, FakeAuctionBuyCancellationScheduler.Instance);

            confirmBuyResult.Should().BeTrue();
            AuctionStateShouldBeValidAfterBuyConfirmed(auction, buyerId);
        }

        [Fact]
        public async Task Does_nothing_when_confirmed_again_by_the_same_buyer()
        {
            var auction = GivenAuctionThatCanBeBought();
            var buyerId = GivenUserId.Build();
            var mockAuctionBuyCancellationScheduler = new Mock<IAuctionBuyCancellationScheduler>();
            await BuyAuction(auction, buyerId, mockAuctionCancellationScheduler: mockAuctionBuyCancellationScheduler);
            auction.ConfirmBuy(auction.Buyer, FakeAuctionBuyCancellationScheduler.Instance);

            var result = auction.ConfirmBuy(auction.Buyer, FakeAuctionBuyCancellationScheduler.Instance);

            result.Should().BeTrue();
            mockAuctionBuyCancellationScheduler.Verify(f => f.ScheduleAuctionBuyCancellation(auction.AggregateId, It.IsAny<TimeOnly>()), Times.Once());
            AuctionStateShouldBeValidAfterBuyConfirmed(auction, buyerId);
        }

        [Fact]
        public async Task Confirmation_fails_when_confirming_buy_for_not_existing_buyer()
        {
            var (auction, _) = await GivenBoughtAuction();
            var notExistingBuyer = Guid.NewGuid();

            var confirmBuyResult = auction.ConfirmBuy(notExistingBuyer, FakeAuctionBuyCancellationScheduler.Instance);

            confirmBuyResult.Should().BeFalse();
            AuctionStateShouldBeValidAfterBuyConfirmationFails(auction, auction.Buyer);
        }

        public static IEnumerable<object[]> UserIdForNotBoughtAuction()
        {
            yield return new[] { UserId.New() }; 
            yield return new[] { UserId.Empty }; 
        }

        [Theory]
        [MemberData(nameof(UserIdForNotBoughtAuction))]
        public void Confirmation_fails_when_confirming_buy_for_not_bought_transaction(UserId notExistingBuyer)
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();

            var confirmBuyResult = auction.ConfirmBuy(notExistingBuyer, FakeAuctionBuyCancellationScheduler.Instance);

            confirmBuyResult.Should().BeFalse();
            AuctionStateShouldBeValidAfterBuyConfirmationFails(auction, UserId.Empty);
        }

        [Fact]
        public async Task Confirmation_fails_when_confirming_buy_for_completed()
        {
            var (auction, _) = await GivenBoughtAuction();
            auction.ConfirmBuy(auction.Buyer, FakeAuctionBuyCancellationScheduler.Instance);
            var notExistingBuyer = Guid.NewGuid();

            var confirmBuyResult = auction.ConfirmBuy(notExistingBuyer, FakeAuctionBuyCancellationScheduler.Instance);

            confirmBuyResult.Should().BeFalse();
            AuctionStateShouldBeValidAfterBuyConfirmed(auction, auction.Buyer);
        }

        #endregion

        #region CancelBuy

        [Fact]
        public async Task Can_be_cancelled_with_valid_buyer_id()
        {
            var (auction, _) = await GivenBoughtAuction();

            var cancelBuyResult = auction.CancelBuy(auction.Buyer, FakeAuctionBuyCancellationScheduler.Instance);

            cancelBuyResult.Should().BeTrue();
            AuctionStateShouldBeValidAfterBuyCanceled(auction);
        }

        [Fact]
        public async Task Cancellation_fails_when_cancelled_with_non_existing_buyer_id()
        {
            var (auction, _) = await GivenBoughtAuction();
            var nonExistingBuyerId = UserId.New();

            var cancelBuyResult = auction.CancelBuy(nonExistingBuyerId, FakeAuctionBuyCancellationScheduler.Instance);

            cancelBuyResult.Should().BeFalse();
            AuctionStateShouldBeValidAfterBuyCancelationFails(auction);
        }

        [Fact]
        public async Task Cancellation_fails_when_cancelled_for_completed_auction()
        {
            var (auction, _) = await GivenBoughtAuction();
            auction.ConfirmBuy(auction.Buyer, FakeAuctionBuyCancellationScheduler.Instance);

            var cancelBuyResult = auction.CancelBuy(auction.Buyer, FakeAuctionBuyCancellationScheduler.Instance);

            cancelBuyResult.Should().BeFalse();
            AuctionStateShouldBeValidAfterBuyConfirmed(auction, auction.Buyer);
        }

        [Fact]
        public async Task Cancellation_fails_when_cancelled_after_bought_by_another_user()
        {
            // Arrange
            var (auction, _) = await GivenBoughtAuction();
            var buyerId1 = auction.Buyer;
            var buyerId2 = GivenUserId.Build();

            //Act
            auction.CancelBuy();
            // scheduled cancellation happened - now anyone can buy
            await BuyAuction(auction, buyerId2);
            var cancelBuyResult = auction.CancelBuy(buyerId1, FakeAuctionBuyCancellationScheduler.Instance);

            //Assert
            cancelBuyResult.Should().BeFalse();
            AuctionStateShouldBeValidAfterBeingBought(auction, buyerId2);
        }

        #endregion


        protected async Task<(Auction Auction, Guid TransactionId)> GivenBoughtAuction(UserId buyerId = null)
        {
            var auction = GivenAuctionThatCanBeBought();
            await BuyAuction(auction, buyerId ?? GivenUserId.Build());
            auction.MarkPendingEventsAsHandled();
            return (auction, auction.Buyer.Value);
        }

        protected Auction GivenAuctionThatCanBeBought()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            auction.MarkPendingEventsAsHandled();
            return auction;
        }

        protected Task BuyAuction(Auction auction, UserId buyerId = null, AuctionPaymentVerificationScenario paymentVerificationScenario = null,
            Mock<IAuctionBuyCancellationScheduler>? mockAuctionCancellationScheduler = null)
        {
            buyerId ??= GivenUserId.Build();
            paymentVerificationScenario ??= AuctionPaymentVerificationContracts.SuccessfulScenario(auction, buyerId);
            var stubPaymentVerification = new GivenAuctionPaymentVerification().Build(paymentVerificationScenario);
            return auction.Buy(buyerId, paymentVerificationScenario.Given.paymentMethod, stubPaymentVerification, 
                mockAuctionCancellationScheduler?.Object ?? Mock.Of<IAuctionBuyCancellationScheduler>());
        }

        private void AuctionStateShouldBeValidAfterBuyCancelationFails(Auction auction)
        {
            auction.Buyer.Should().NotBe(UserId.Empty);
            auction.Completed.Should().BeFalse();
        }

        private void AuctionStateShouldBeValidAfterBuyCanceled(Auction auction)
        {
            auction.Buyer.Should().Be(UserId.Empty);
            auction.Completed.Should().BeFalse();
        }

        private static void AuctionStateShouldBeValidAfterBuyConfirmationFails(Auction auction, UserId expectedBuyerId)
        {
            auction.Buyer.Should().Be(expectedBuyerId);
            auction.Completed.Should().BeFalse();
        }

        private static void AuctionStateShouldBeValidAfterBuyConfirmed(Auction auction, UserId expectedBuyerId)
        {
            auction.Buyer.Should().Be(expectedBuyerId);
            auction.Completed.Should().BeTrue();
        }

        private static void AuctionStateShouldBeValidAfterBeingBought(Auction auction, UserId buyerId)
        {
            auction.Buyer.Should().Be(buyerId);
            auction.Completed.Should().BeFalse();
        }
    }
}
