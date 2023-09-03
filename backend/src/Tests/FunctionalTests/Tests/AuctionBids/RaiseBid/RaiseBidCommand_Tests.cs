using AuctionBids.Domain.Repositories;
using Auctions.Domain.Repositories;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Common.Application.Events;
using Core.Command.Bid;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using FunctionalTests.Commands;
using FunctionalTests.Tests.Auctions.CreateAuction;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using Users.Domain.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace FunctionalTests.Tests.AuctionBids.RaiseBid
{
    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class RaiseBidCommand_Tests : TestBase
    {
        public RaiseBidCommand_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "ReadModel.Core", "ReadModel.Contracts", "AuctionBids.Application", "Auctions.Application", "UserPayments.Application", "Users.Application")
        {
        }

        private IAppEvent<Event> BuildAppEvent(IAppEventBuilder builder, Event @event)
        {
            return builder.WithCommandContext(Common.Application.Commands.CommandContext.CreateNew("test"))
                .WithEvent(@event).Build<Event>();
        }

        [Fact]
        public async Task RaiseBidCommand_test()
        {
            ChangeSignedInUser(2000);

            var auctions = ServiceProvider.GetRequiredService<IAuctionRepository>();
            var auctionArgs = new GivenAuctionArgs().WithOwner(new(SignedInUser.AggregateId.Value)).Build();
            var auction = new GivenAuction().WithAuctionArgs(auctionArgs).Build();
            auctions.AddAuction(auction);

            var allAuctionBids = ServiceProvider.GetRequiredService<IAuctionBidsRepository>();
            var auctionBids = global::AuctionBids.Domain.AuctionBids.CreateNew(new(auction.AggregateId), new(1, 2, 3), new(auction.Owner));
            ReadModelDbContext.AuctionBidsReadModel.InsertOne(new()
            {
                AuctionId = auction.AggregateId.ToString(),
                OwnerId = auction.Owner.ToString(),
                AuctionBidsId = auctionBids.AggregateId.ToString(),
            });
            allAuctionBids.Add(auctionBids);

            var eventBus = ServiceProvider.GetRequiredService<IEventBus>();
            var appEventBuilder = ServiceProvider.GetRequiredService<IAppEventBuilder>();


            foreach (var e in auction.PendingEvents.Select(e => BuildAppEvent(appEventBuilder, e)))
            {
                await eventBus.Publish(e);
            }

            AssertEventual(() =>
            {
                var userReadCreated = ReadModelDbContext.UsersReadModel.Find(u => u.UserIdentity.UserId == SignedInUser.AggregateId.Value.ToString()).FirstOrDefault()
                != null;
                var auctionReadCreated = ReadModelDbContext.AuctionsReadModel.Find(a => a.AuctionId == auction.AggregateId.Value.ToString()).FirstOrDefault()
                != null;
                var auctionBidsCreated = ReadModelDbContext.AuctionBidsReadModel.Find(b => b.AuctionId == auction.AggregateId.Value.ToString()).FirstOrDefault()
                != null;
                return userReadCreated && auctionReadCreated && auctionBidsCreated;
            });


            ChangeSignedInUser(2000, "test");
            var cmd = new RaiseBidCommand(auction.AggregateId.Value, auctionBids.CurrentPrice + 1);
            await SendCommand(cmd);

            AssertEventual(() =>
            {
                var auctionBidsRead = ReadModelDbContext.AuctionBidsReadModel.Find(b => b.AuctionId == auction.AggregateId.Value.ToString()).FirstOrDefault();
                var auctionRead = ReadModelDbContext.AuctionsReadModel.Find(a => a.AuctionId == auction.AggregateId.Value.ToString()).FirstOrDefault();
                var userRead = ReadModelDbContext.UsersReadModel.Find(u => u.UserIdentity.UserId == cmd.SignedInUser.ToString()).FirstOrDefault();

                var priceRaised = auctionRead?.ActualPrice == cmd.Price;
                var winnerSet = auctionRead?.Winner?.UserId == cmd.SignedInUser.ToString() && auctionRead?.Winner.UserName == "test";
                var winnerBidSet = auctionRead?.WinningBid != null;
                var userBidAdded = userRead.UserBids.Count == 1 && userRead.UserBids[0].AuctionId == auction.AggregateId.ToString();

                return priceRaised && winnerSet && winnerBidSet && userBidAdded;
            });
        }
    }
}
