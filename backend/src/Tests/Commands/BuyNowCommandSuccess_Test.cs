using Auctions.Application.Commands.BuyNow;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Builders.CreateAuctionCommandBuilder;
using static UserPayments.DomainEvents.Events.V1;

namespace FunctionalTests.Commands
{
    using MongoDB.Driver;
    using UserPayments.Domain;
    using UserPayments.Domain.Events;
    using Users.Domain.Events;
    using Users.DomainEvents;

    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class BuyNowCommandSuccess_Test : BuyNowCommandTestBase
    {
        readonly Type[] SuccessExpectedEvents = new[] {
                    typeof(Auctions.DomainEvents.AuctionLocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXStarted),
                    typeof(BuyNowPaymentCreated),
                    typeof(LockedFundsCreated),
                    typeof(UserCreditsLockedForBuyNowAuction),
                    typeof(BuyNowPaymentConfirmed),
                    typeof(PaymentStatusChangedToConfirmed),
                    typeof(Auctions.DomainEvents.AuctionUnlocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXSuccess),
                    typeof(PaymentStatusChangedToCompleted),
                    typeof(CreditsWithdrawn),
                };

        public BuyNowCommandSuccess_Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task BuyNowCommand_creates_confirmed_payment_and_sets_auction_to_completed()
        {
            var createAuctionCommand = GivenCreateAuctionCommand().Build();
            await CreateAuction(createAuctionCommand);
            var initialCredits = 1000m;
            var user = await CreateUser(initialCredits);
            CreateUserPayments(user);

            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = auctions.All.First(a => a.Owner.Value == createAuctionCommand.SignedInUser).AggregateId,
            };
            var status = await SendCommand(buyNowCommand);

            AssertEventual(() =>
            {
                var auctionAssertion = AuctionShouldBeCompleted(buyNowCommand);
                var (sagaCompleted, allEventsProcessed) = SagaShouldBeCompletedAndAllEventsShouldBeProcessed(status);
                var expectedEventsAssertion = ExpectedEventsShouldBePublished(SuccessExpectedEvents);
                var paymentCompletedAssertion = PaymentStatusShouldBe(user, buyNowCommand, PaymentStatus.Completed);
                var userCreditsAssertion = UserCreditsShouldBe(initialCredits - createAuctionCommand.BuyNowPrice!, user);
                var userReadCreditsAssertion = UserReadCreditsShouldBe(initialCredits - createAuctionCommand.BuyNowPrice!, user);

                return sagaCompleted &&
                allEventsProcessed &&
                auctionAssertion &&
                expectedEventsAssertion &&
                paymentCompletedAssertion &&
                userCreditsAssertion &&
                userReadCreditsAssertion;
            });
        }
    }
}
