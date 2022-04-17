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
    using Auctions.Application.Commands.CreateAuction;
    using Auctions.Domain;
    using Auctions.Tests.Base.Mocks;
    using Core.Common.Domain;
    using Core.Common.Domain.Users;
    using MongoDB.Driver;
    using UserPayments.Domain;
    using UserPayments.Domain.Events;
    using UserPayments.Domain.Shared;
    using Users.Domain;
    using Users.Domain.Events;
    using Users.Domain.Repositories;
    using Users.DomainEvents;
    using Users.Tests.Base.Mocks;

    public class BuyNowCommandTestBase : TestBase, IDisposable
    {
        protected InMemoryAuctionRepository auctions;
        protected InMemoryAuctionBidsRepository auctionBids;
        protected InMemortUserPaymentsRepository allUserPayments;
        protected InMemoryUserRepository users;
        protected ITestOutputHelper outputHelper;


        public BuyNowCommandTestBase(ITestOutputHelper outputHelper) 
            : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "UserPayments.Application", "Users.Application")
        {
            this.outputHelper = outputHelper;
            auctions = (InMemoryAuctionRepository)ServiceProvider.GetRequiredService<IAuctionRepository>();
            auctionBids = (InMemoryAuctionBidsRepository)ServiceProvider.GetRequiredService<IAuctionBidsRepository>();
            allUserPayments = (InMemortUserPaymentsRepository)ServiceProvider.GetRequiredService<IUserPaymentsRepository>();
            users = (InMemoryUserRepository)ServiceProvider.GetRequiredService<IUserRepository>();
        }

      

        protected async Task CreateAuction(CreateAuctionCommand cmd)
        {
            //start session
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);
            await Task.Delay(3000);
            //create auction in session
            await SendCommand(cmd);
            await Task.Delay(3000);
            AssertEventual(() =>
            {
                var createdAuction = auctions.All.First();
                var auctionLocked = (createdAuction.Locked);
                var idEqual = (auctionBids.All.Count > 0 && auctionBids.All.First().AuctionId.Value == createdAuction.AggregateId.Value);
                return !auctionLocked && idEqual;
            });
            InMemoryEventBusDecorator.ClearSentEvents();
        }

        protected bool UserReadCreditsShouldBe(decimal credits, User user)
        {
            var userRead = ReadModelDbContext.UsersReadModel.Find(u => u.UserIdentity.UserId == user.AggregateId.ToString()).FirstOrDefault();
            return userRead?.Credits == credits;
        }

        protected bool AuctionShouldBeCompleted(BuyNowCommand buyNowCommand)
        {
            var auction = auctions.All.FirstOrDefault(a => a.AggregateId == buyNowCommand.AuctionId);
            return auction?.Completed == true;
        }

        protected static bool UserCreditsShouldBe(decimal credits, User user)
        {
            return user.Credits == credits;
        }

        protected bool AssertAuctionOnFailure(BuyNowCommand buyNowCommand)
        {
            var auction = auctions.All.FirstOrDefault(a => a.AggregateId == buyNowCommand.AuctionId);
            return auction?.Completed == false;
        }

        protected bool PaymentStatusShouldBe(User user, BuyNowCommand buyNowCommand, PaymentStatus paymentStatus)
        {
            return allUserPayments.All
                                    .FirstOrDefault(u => u.UserId.Value == user.AggregateId.Value)
                                    ?.Payments.FirstOrDefault(p => p.PaymentTargetId?.Value == buyNowCommand.AuctionId)?.Status == paymentStatus;
        }

        protected async Task<User> CreateUser(decimal initialCredits)
        {
            var user = User.Create(await Username.Create("test user"), initialCredits);
            users.AddUser(user);
            user.MarkPendingEventsAsHandled();
            ChangeSignedInUser(user.AggregateId.Value);
            return user;
        }

        protected void CreateUserPayments(User user)
        {
            var userPayments = UserPayments.CreateNew(new global::UserPayments.Domain.Shared.UserId(user.AggregateId.Value));
            userPayments.MarkPendingEventsAsHandled();
            allUserPayments.Add(userPayments);
        }

        protected bool AssertUser(User user, Auction boughtAuction, decimal initialCredits)
        {
            return user.Credits == initialCredits - boughtAuction.BuyNowPrice!.Value;
        }

        protected bool ExpectedEventsShouldBePublished(Type[] expectedEvents)
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

        public override void Dispose()
        {
            base.Dispose();
            TruncateReadModelNotificaitons(ServiceProvider);
        }

    }
}
