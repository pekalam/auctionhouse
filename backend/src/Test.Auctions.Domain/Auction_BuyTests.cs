using Auctions.Domain;
using Auctions.Domain.Services;
using Core.DomainFramework;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Auctions.DomainEvents.Events.V1;
using static Test.Auctions.Domain.AuctionTestConstants;

namespace Test.Auctions.Domain
{
    public record AuctionPaymentVerificationScenario(AuctionPaymentVerificationContractArgs Given, bool Expected);

    public static class AuctionPaymentVerificationContracts
    {
        public static AuctionPaymentVerificationScenario ValidParams(Auction auction, UserId userId)
        {
            var given = new AuctionPaymentVerificationContractArgs { auction = auction, buyer = userId, paymentMethod = "test" };
            var expected = true;
            return new(given, expected);
        }
    }

    public class AuctionPaymentVerificationContractArgs
    {
        public Auction auction; public UserId buyer; public string paymentMethod;
    }

    public class GivenAuctionPaymentVerification
    {
        public IAuctionPaymentVerification Create(AuctionPaymentVerificationScenario scenario)
        {
            var mock = new Moq.Mock<IAuctionPaymentVerification>();
            mock.Setup(f => f.Verification(scenario.Given.auction, scenario.Given.buyer, scenario.Given.paymentMethod)).Returns(Task.FromResult(scenario.Expected));
            return mock.Object;
        }
    }

    public static class AuctionTestConstants
    {
        public const string PRODUCT_NAME = "test_name";
        public const string PRODUCT_DESCRIPTION = "desccription 1111";
        public const string CATEGORY_NAME = "test";
        public static readonly string[] CATEGORY_IDS = new string[3] {"1","2","3"};
        public const string TAG_1 = "tag1";
        public const string NAME = "Test name";
        public const decimal BUY_NOW_PRICE = 90.0m;
    }

    internal class TestConvertCategoryNamesToRootToLeafIds : IConvertCategoryNamesToRootToLeafIds
    {
        public Task<CategoryId[]> ConvertNames(string[] categoryNames)
        {
            return Task.FromResult(CATEGORY_IDS.Select(c => new CategoryId(Convert.ToInt32(c))).ToArray());
        }
    }

    public class GivenAuction
    {
        public Auction ValidOfTypeBuyNowAndBid()
        {
            var auctionArgsBuilder = new AuctionArgs.Builder()
                .SetBuyNow(BUY_NOW_PRICE)
                .SetStartDate(DateTime.UtcNow.AddMinutes(20))
                .SetEndDate(DateTime.UtcNow.AddDays(5))
                .SetOwner(UserId.New())
                .SetProduct(new Product(PRODUCT_NAME, PRODUCT_DESCRIPTION, Condition.New))
                .SetBuyNowOnly(false)
                .SetTags(new[] { TAG_1 })
                .SetName(NAME);
            auctionArgsBuilder.SetCategories(CATEGORY_IDS, new TestConvertCategoryNamesToRootToLeafIds()).GetAwaiter().GetResult();
            return new Auction(auctionArgsBuilder.Build());
        }
    }

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
