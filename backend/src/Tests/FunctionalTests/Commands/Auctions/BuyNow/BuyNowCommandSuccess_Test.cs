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
    using Auctions.Domain;
    using Auctions.Domain.Repositories;
    using Common.Application;
    using Core.Common.Domain.Users;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using Users.Domain.Events;
    using Users.DomainEvents;

    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class BuyNowCommandSuccess_Test : BuyNowCommandTestBase
    {
        readonly Type[] SuccessExpectedEvents = new[] {
                    typeof(Auctions.DomainEvents.AuctionLocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXStarted),
                    typeof(UserPayments.Domain.Events.BuyNowPaymentCreated),
                    typeof(LockedFundsCreated),
                    typeof(UserCreditsLockedForBuyNowAuction),
                    typeof(BuyNowPaymentConfirmed),
                    typeof(UserPayments.Domain.Events.PaymentStatusChangedToConfirmed),
                    typeof(Auctions.DomainEvents.AuctionUnlocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXSuccess),
                    typeof(UserPayments.Domain.Events.PaymentStatusChangedToCompleted),
                    typeof(CreditsWithdrawn),
                };

        public BuyNowCommandSuccess_Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task BuyNowCommand_creates_confirmed_payment_and_sets_auction_to_completed()
        {
            var createAuctionCommand = GivenCreateAuctionCommand().Build();
            var createdAuction = await CreateAuction(createAuctionCommand);
            var initialCredits = 1000m;
            var user = await CreateUser(initialCredits);
            user.MarkPendingEventsAsHandled();
            CreateUserPayments(user);

            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = Guid.Parse(createdAuction.AuctionId),
            };
            var status = await SendCommand(buyNowCommand);

            AssertEventual(new BuyNowCommandProbe(this, buyNowCommand.AuctionId, status, SuccessExpectedEvents, user, createAuctionCommand.BuyNowPrice, initialCredits, outputHelper).Check);
        }
    }


    public class BuyNowCommandProbe
    {
        private readonly TestBase _testBase;
        private readonly Guid _auctionId;
        private readonly RequestStatus _status;
        private readonly Type[] _expectedEvents;
        private readonly User _user;
        private readonly ITestOutputHelper outputHelper;
        private readonly BuyNowPrice _buyNowPrice;
        private readonly decimal _initialCredits;


        public BuyNowCommandProbe(TestBase testBase, Guid auctionId, RequestStatus status, Type[] expectedEvents, User user, BuyNowPrice buyNowPrice, decimal initialCredits, ITestOutputHelper outputHelper)
        {
            _testBase = testBase;
            _auctionId = auctionId;
            _status = status;
            _expectedEvents = expectedEvents;
            _user = user;
            _buyNowPrice = buyNowPrice;
            _initialCredits = initialCredits;
            this.outputHelper = outputHelper;
        }

        public bool Check()
        {
            var auctions = _testBase.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var auction = auctions.FindAuction(_auctionId);
            var allUserPayments = _testBase.ServiceProvider.GetRequiredService<UserPayments.Domain.Repositories.IUserPaymentsRepository>();

            var auctionCompleted = auction?.Completed == true;
            var (sagaCompleted, allEventsProcessed) = _testBase.SagaShouldBeCompletedAndAllEventsShouldBeProcessed(_status);
            var expectedEventsAssertion = ExpectedEventsShouldBePublished(_expectedEvents);
            var paymentCompletedAssertion = PaymentStatusShouldBe(UserPayments.Domain.PaymentStatus.Completed);
            var userCreditsAssertion = UserCreditsShouldBe(_initialCredits - _buyNowPrice);
            var userReadCreditsAssertion = UserReadCreditsShouldBe(_initialCredits - _buyNowPrice);

            return sagaCompleted &&
            allEventsProcessed &&
            auctionCompleted &&
            expectedEventsAssertion &&
            paymentCompletedAssertion &&
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
