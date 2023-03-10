using Auctions.Application.Commands.BuyNow;
using FunctionalTests.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Tests.Auctions.Helpers.TestCreateAuctionCommandBuilder;
using static UserPayments.DomainEvents.Events.V1;

namespace FunctionalTests.Tests.Auctions.BuyNow
{
    using Common.Application;
    using Core.Common.Domain.Users;
    using FunctionalTests.Tests.Auctions.CreateAuction;
    using global::Auctions.Domain.Repositories;
    using global::Auctions.Tests.Base.Domain.Services.TestDoubleBuilders;
    using global::Users.DomainEvents;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;


    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class BuyNowCommandFailure_Test : BuyNowCommandTestBase, IDisposable
    {
        readonly Type[] FailureExpectedEvents = new[] {
                    typeof(global::Auctions.DomainEvents.Events.V1.AuctionBought),
                    typeof(UserPayments.Domain.Events.BuyNowPaymentCreated),
                    typeof(UserCreditsFailedToLockForBuyNowAuction),
                    typeof(BuyNowPaymentFailed),
                    typeof(UserPayments.Domain.Events.PaymentStatusChangedToFailed),
                    typeof(global::Auctions.DomainEvents.Events.V1.AuctionBuyCanceled),
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
        public async Task BuyNowCommand_creates_failed_payment_and_cancels_buy_when_user_doesnt_have_enought_credits()
        //case when there is not enough credits at the time the request is handled by UserPayments
        {
            var createAuctionCommand = GivenCreateAuctionCommand().Build();
            var createdAuction = await CreateAuction(createAuctionCommand);
            var initialCredits = 0m;
            var user = await CreateUser(initialCredits);
            CreateUserPayments(user);

            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = Guid.Parse(createdAuction.AuctionId),
            };
            var status = await SendCommand(buyNowCommand);

            AssertEventual(
                new BuyNowCommandFailureProbe(this, FailureExpectedEvents, buyNowCommand.AuctionId, status, user, initialCredits).Check);
        }
    }

    public class BuyNowCommandFailureProbe
    {
        private readonly TestBase _testBase;
        private readonly Type[] _expectedEvents;
        private readonly Guid _auctionId;
        private readonly RequestStatus _status;
        private readonly User _user;
        private readonly decimal _initialCredits;

        public BuyNowCommandFailureProbe(TestBase testBase, Type[] expectedEvents, Guid auctionId, RequestStatus status, User user, decimal initialCredits)
        {
            _testBase = testBase;
            _expectedEvents = expectedEvents;
            _auctionId = auctionId;
            _status = status;
            _user = user;
            _initialCredits = initialCredits;
        }

        public bool Check()
        {
            var auctions = _testBase.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var expectedEventsAssertion = _testBase.ExpectedEventsShouldBePublished(_expectedEvents);
            var auction = auctions.FindAuction(_auctionId);
            var (sagaCompleted, allEventsProcessed) = _testBase.SagaShouldBeCompletedAndAllEventsShouldBeProcessed(_status);
            var allUserPayments = _testBase.ServiceProvider.GetRequiredService<UserPayments.Domain.Repositories.IUserPaymentsRepository>();

            var auctionNotCompleted = auction?.Completed == false;
            var paymentStatusAssertion = PaymentStatusShouldBe(UserPayments.Domain.PaymentStatus.Failed);
            var userCreditsAssertion = UserCreditsShouldBe(_initialCredits);
            var userReadCreditsAssertion = UserReadCreditsShouldBe(_initialCredits);

            return auctionNotCompleted &&
            expectedEventsAssertion &&
            paymentStatusAssertion &&
            userCreditsAssertion &&
            userReadCreditsAssertion;

            bool PaymentStatusShouldBe(UserPayments.Domain.PaymentStatus paymentStatus)
            {
                return allUserPayments.WithUserId(new UserPayments.Domain.Shared.UserId(_user.AggregateId.Value)).GetAwaiter().GetResult()
                                        ?.Payments.FirstOrDefault(p => p.PaymentTargetId?.Value == _auctionId)?.Status == paymentStatus;
            }

            bool UserReadCreditsShouldBe(decimal credits)
            {
                var userRead = _testBase.ReadModelDbContext.UsersReadModel.Find(u => u.UserIdentity.UserId == _user.AggregateId.ToString()).FirstOrDefault();
                return userRead?.Credits == credits;
            }

            bool UserCreditsShouldBe(decimal credits)
            {
                return _user.Credits == credits;
            }
        }
    }
}
