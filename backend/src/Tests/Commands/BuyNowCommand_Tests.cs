using AuctionBids.Domain.Repositories;
using Auctions.Application.Commands.BuyNow;
using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Domain.Repositories;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserPayments.Domain.Repositories;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Builders.CreateAuctionCommandBuilder;
using static UserPayments.DomainEvents.Events.V1;

namespace FunctionalTests.Commands
{
    using Auctions.Domain;
    using Core.Common.Domain;
    using Core.Common.Domain.Users;
    using Test.Auctions.Base.Mocks;
    using Test.Users.Base.Mocks;
    using UserPayments.Domain;
    using UserPayments.Domain.Events;
    using UserPayments.Domain.Shared;
    using Users.Domain;
    using Users.Domain.Events;
    using Users.Domain.Repositories;
    using Users.DomainEvents;

    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class BuyNowCommand_Tests : TestBase, IDisposable
    {
        InMemoryAuctionRepository auctions;
        InMemoryAuctionBidsRepository auctionBids;
        InMemortUserPaymentsRepository allUserPayments;
        InMemoryUserRepository users;
        ITestOutputHelper outputHelper;

        Type[] SuccessExpectedEvents = new[] {
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

        Type[] FailureExpectedEvents = new[] {
                    typeof(Auctions.DomainEvents.AuctionLocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXStarted),
                    typeof(BuyNowPaymentCreated),
                    typeof(UserCreditsFailedToLockForBuyNowAuction),
                    typeof(BuyNowPaymentFailed),
                    typeof(PaymentStatusChangedToFailed),
                    typeof(Auctions.DomainEvents.AuctionUnlocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXCanceled),
                };

        public BuyNowCommand_Tests(ITestOutputHelper outputHelper) 
            : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "UserPayments.Application", "Users.Application")
        {
            this.outputHelper = outputHelper;
            auctions = (InMemoryAuctionRepository)ServiceProvider.GetRequiredService<IAuctionRepository>();
            auctionBids = (InMemoryAuctionBidsRepository)ServiceProvider.GetRequiredService<IAuctionBidsRepository>();
            allUserPayments = (InMemortUserPaymentsRepository)ServiceProvider.GetRequiredService<IUserPaymentsRepository>();
            users = (InMemoryUserRepository)ServiceProvider.GetRequiredService<IUserRepository>();
        }

        [Fact]
        public async Task BuyNowCommand_creates_confirmed_payment_and_sets_auction_to_completed()
        {
            await StartAuctionCreateSession();
            await CreateAuction();

            AssertAuctionCreated();
            InMemoryEventBusDecorator.ClearSentEvents();

            var initialCredits = 1000m;
            var user = await CreateUser(initialCredits);

            CreateUserPayments(user);

            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = auctions.All.First().AggregateId,
            };
            await SendCommand(buyNowCommand);

            AssertEventual(() =>
            {
                var auction = auctions.All.FirstOrDefault(a => a.AggregateId == buyNowCommand.AuctionId);
                var auctionCompleted = auction?.Completed == true;

                return AssertExpectedEventsArePublished(SuccessExpectedEvents) && AssertPaymentStatus(user, buyNowCommand, PaymentStatus.Completed) &&
                    auction is not null && auctionCompleted && AssertUser(user, auction, initialCredits);
            });
        }

        [Fact]
        public async Task BuyNowCommand_creates_failed_payment_and_cancels_buynowtx_when_user_doesnt_have_enought_credits() 
            //TODO: this case should be handled in BuyNowCommandHandler rather than asynchronously because it's too chatty and results in temporarily locked auction
        {
            await StartAuctionCreateSession();
            await CreateAuction();

            AssertAuctionCreated();
            InMemoryEventBusDecorator.ClearSentEvents();

            var initialCredits = 0;
            var user = await CreateUser(initialCredits);

            CreateUserPayments(user);

            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = auctions.All.First().AggregateId,
            };
            await SendCommand(buyNowCommand);

            AssertEventual(() =>
            {
                var auction = auctions.All.FirstOrDefault(a => a.AggregateId == buyNowCommand.AuctionId);
                var auctionNotCompleted = auction?.Completed == false;

                return AssertExpectedEventsArePublished(FailureExpectedEvents) && AssertPaymentStatus(user, buyNowCommand, PaymentStatus.Failed) &&
                    auction is not null && auctionNotCompleted && user.Credits == initialCredits;
            });
        }

        private bool AssertPaymentStatus(User user, BuyNowCommand buyNowCommand, PaymentStatus paymentStatus)
        {
            return allUserPayments.All
                                    .FirstOrDefault(u => u.UserId.Value == user.AggregateId.Value)
                                    ?.Payments.FirstOrDefault(p => p.PaymentTargetId?.Value == buyNowCommand.AuctionId)?.Status == paymentStatus;
        }

        private async Task<User> CreateUser(decimal initialCredits)
        {
            var user = User.Create(await Username.Create("test user"), initialCredits);
            users.AddUser(user);
            user.MarkPendingEventsAsHandled();
            ChangeSignedInUser(user.AggregateId.Value);
            return user;
        }

        private void CreateUserPayments(User user)
        {
            var userPayments = UserPayments.CreateNew(new global::UserPayments.Domain.Shared.UserId(user.AggregateId.Value));
            userPayments.MarkPendingEventsAsHandled();
            allUserPayments.Add(userPayments);
        }

        private void AssertAuctionCreated()
        {
            AssertEventual(() =>
            {
                var createdAuction = auctions.All.First();
                var auctionLocked = (createdAuction.Locked);
                var idEqual = (auctionBids.All.Count > 0 && auctionBids.All.First().AuctionId.Value == createdAuction.AggregateId.Value);
                return !auctionLocked && idEqual;
            });
        }

        private async Task CreateAuction()
        {
            //create auction in session
            var createAuctionCmd = GivenCreateAuctionCommand().Build();
            await SendCommand(createAuctionCmd);
            await Task.Delay(3000);
        }

        private async Task StartAuctionCreateSession()
        {
            //start session
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);
            await Task.Delay(3000);
        }

        private bool AssertUser(User user, Auction boughtAuction, decimal initialCredits)
        {
            return user.Credits == initialCredits - boughtAuction.BuyNowPrice!.Value;
        }

        private bool AssertExpectedEventsArePublished(Type[] expectedEvents)
        {
            var allEventsPublished = SentEvents.Select(e => e.Event.GetType()).Except(expectedEvents).Any() == false;

            if (SentEvents.Count > expectedEvents.Length)
            {
                outputHelper.WriteLine("Not all events were included in expected");
                foreach (var ev in SentEvents.Select(e => e.Event.GetType()).Except(expectedEvents))
                {
                    outputHelper.WriteLine("Event: " + ev.Name);
                }
            }
            else if (!allEventsPublished)
            {
                var notPublished = expectedEvents.Except(SentEvents.Select(e => e.Event.GetType()));
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

        public void Dispose()
        {
            TruncateReadModelNotificaitons(ServiceProvider);
        }

    }
}
