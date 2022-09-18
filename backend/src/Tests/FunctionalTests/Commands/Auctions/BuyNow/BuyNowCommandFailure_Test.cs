using Auctions.Application.Commands.BuyNow;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Commands.TestCreateAuctionCommandBuilder;
using static UserPayments.DomainEvents.Events.V1;

namespace FunctionalTests.Commands
{
    using Auctions.Domain.Repositories;
    using Auctions.Tests.Base.Domain.Services.Fakes;
    using Auctions.Tests.Base.Domain.Services.TestDoubleBuilders;
    using Common.Application;
    using Core.Common.Domain.Users;
    using FunctionalTests.Mocks;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using ReadModel.Core.Queries.User.UserAuctions;
    using Users.DomainEvents;


    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class BuyNowCommandFailure_Test : BuyNowCommandTestBase, IDisposable
    {
        readonly Type[] FailureExpectedEvents = new[] {
                    typeof(Auctions.DomainEvents.AuctionLocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXStarted),
                    typeof(UserPayments.Domain.Events.BuyNowPaymentCreated),
                    typeof(UserCreditsFailedToLockForBuyNowAuction),
                    typeof(BuyNowPaymentFailed),
                    typeof(UserPayments.Domain.Events.PaymentStatusChangedToFailed),
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
                new BuyNowCommandFailureProbe(this, FailureExpectedEvents, outputHelper, buyNowCommand.AuctionId, status, user, initialCredits).Check);
        }
    }

    public class BuyNowCommandFailureProbe
    {
        private readonly TestBase _testBase;
        private readonly ITestOutputHelper outputHelper;
        private readonly Type[] _expectedEvents;
        private readonly Guid _auctionId;
        private readonly RequestStatus _status;
        private readonly User _user;
        private readonly decimal _initialCredits;

        public BuyNowCommandFailureProbe(TestBase testBase, Type[] expectedEvents, ITestOutputHelper outputHelper, Guid auctionId, RequestStatus status, User user, decimal initialCredits)
        {
            _testBase = testBase;
            _expectedEvents = expectedEvents;
            this.outputHelper = outputHelper;
            _auctionId = auctionId;
            _status = status;
            _user = user;
            _initialCredits = initialCredits;
        }

        public bool Check()
        {
            var auctions = _testBase.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var expectedEventsAssertion = ExpectedEventsShouldBePublished(_expectedEvents);
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

        private bool ExpectedEventsShouldBePublished(Type[] expectedEvents)
        {
            var allEventsPublished = _testBase.SentEvents.Select(e => e.Event.GetType()).Except(expectedEvents).Any() == false;

            if (_testBase.SentEvents.Count > expectedEvents.Length)
            {
                outputHelper.WriteLine("Not all events were included in expected");
                foreach (var ev in _testBase.SentEvents.Select(e => e.Event.GetType()).Except(expectedEvents))
                {
                    outputHelper.WriteLine("Event: " + ev.Name);
                }
            }
            else if (!allEventsPublished)
            {
                var notPublished = expectedEvents.Except(_testBase.SentEvents.Select(e => e.Event.GetType()));
                outputHelper.WriteLine($"Not all expected events were published ({notPublished.Count()}/{expectedEvents.Length}):");
                foreach (var ev in notPublished)
                {
                    outputHelper.WriteLine("Not published: " + ev.Name);
                }
            }
            else
            {
                outputHelper.WriteLine("All events were published");
            }

            return allEventsPublished;
        }
    }
}
