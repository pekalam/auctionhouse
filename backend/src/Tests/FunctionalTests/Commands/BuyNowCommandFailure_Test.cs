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
    using Auctions.Tests.Base.Domain.Services.TestDoubleBuilders;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using UserPayments.Domain;
    using UserPayments.Domain.Events;
    using Users.DomainEvents;


    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class BuyNowCommandFailure_Test : BuyNowCommandTestBase, IDisposable
    {
        readonly Type[] FailureExpectedEvents = new[] {
                    typeof(Auctions.DomainEvents.AuctionLocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXStarted),
                    typeof(BuyNowPaymentCreated),
                    typeof(UserCreditsFailedToLockForBuyNowAuction),
                    typeof(BuyNowPaymentFailed),
                    typeof(PaymentStatusChangedToFailed),
                    typeof(Auctions.DomainEvents.AuctionUnlocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXCanceled),
                };

        public BuyNowCommandFailure_Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override void AddCustomServices(IServiceCollection services)
        {
            base.AddCustomServices(services);
            services.AddTransient(s => new GivenAuctionPaymentVerification().BuildAlwaysValidMock().Object);
        }

        [Fact]
        public async Task BuyNowCommand_creates_failed_payment_and_cancels_buynowtx_when_user_doesnt_have_enought_credits()
        //case when there is not enough credits at the time the request is handled by UserPayments
        {
            var createAuctionCommand = GivenCreateAuctionCommand().Build();
            await CreateAuction(createAuctionCommand);
            var initialCredits = 0m;
            var user = await CreateUser(initialCredits);
            CreateUserPayments(user);

            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = auctions.All.First(a => a.Owner.Value == createAuctionCommand.SignedInUser).AggregateId,
            };
            var status = await SendCommand(buyNowCommand);

            AssertEventual(() =>
            {
                var auctionAssertion = AssertAuctionOnFailure(buyNowCommand);
                var (sagaCompleted, allEventsProcessed) = SagaShouldBeCompletedAndAllEventsShouldBeProcessed(status);
                var expectedEventsAssertion = ExpectedEventsShouldBePublished(FailureExpectedEvents);
                var paymentStatusAssertion = PaymentStatusShouldBe(user, buyNowCommand, PaymentStatus.Failed);
                var userCreditsAssertion = UserCreditsShouldBe(initialCredits, user);
                var userReadCreditsAssertion = UserReadCreditsShouldBe(initialCredits, user);

                return auctionAssertion &&
                expectedEventsAssertion &&
                paymentStatusAssertion &&
                userCreditsAssertion &&
                userReadCreditsAssertion;
            });
        }
    }
}
