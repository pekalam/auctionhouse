using Auctions.Application.Commands.BuyNow;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Builders.TestCreateAuctionCommandBuilder;
using static UserPayments.DomainEvents.Events.V1;

namespace FunctionalTests.Commands
{
    using Auctions.Domain;
    using Auctions.Domain.Repositories;
    using Auctions.Tests.Base.Domain.Services.Fakes;
    using Common.Application;
    using Core.Common.Domain.Users;
    using FunctionalTests.Mocks;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using UserPayments.Domain;
    using UserPayments.Domain.Events;
    using UserPayments.Domain.Repositories;
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
            var auctions = (FakeAuctionRepository)_testBase.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var auction = auctions.All.FirstOrDefault(a => a.AggregateId == _auctionId);
            var allUserPayments = (InMemortUserPaymentsRepository)_testBase.ServiceProvider.GetRequiredService<IUserPaymentsRepository>();

            var auctionCompleted = auction?.Completed == true;
            var (sagaCompleted, allEventsProcessed) = _testBase.SagaShouldBeCompletedAndAllEventsShouldBeProcessed(_status);
            var expectedEventsAssertion = ExpectedEventsShouldBePublished(_expectedEvents);
            var paymentCompletedAssertion = PaymentStatusShouldBe(PaymentStatus.Completed);
            var userCreditsAssertion = UserCreditsShouldBe(_initialCredits - _buyNowPrice);
            var userReadCreditsAssertion = UserReadCreditsShouldBe(_initialCredits - _buyNowPrice);

            return sagaCompleted &&
            allEventsProcessed &&
            auctionCompleted &&
            expectedEventsAssertion &&
            paymentCompletedAssertion &&
            userCreditsAssertion &&
            userReadCreditsAssertion;


            bool PaymentStatusShouldBe(PaymentStatus paymentStatus)
            {
                return allUserPayments.All
                                        .FirstOrDefault(u => u.UserId.Value == _user.AggregateId.Value)
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
