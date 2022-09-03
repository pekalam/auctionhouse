using AuctionBids.DomainEvents;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Query.EventHandlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using ReadModel.Core.EventConsumers;
using ReadModel.Core.Model;
using ReadModel.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Tests.Base.Mocks.Events;
using Xunit;

namespace Test.ReadModel.Integration
{
    public class AuctionPriceRisedEventConsumer_IdempotencyTest
    {
        ReadModelDbContext dbContext;
        AuctionRead auction;
        Guid winnerId = Guid.NewGuid();
        Guid bidId = Guid.NewGuid();
        Guid auctionBidsId = Guid.NewGuid();

        public AuctionPriceRisedEventConsumer_IdempotencyTest()
        {
            dbContext = new ReadModelDbContext(
            new MongoDbSettings
            {
                ConnectionString = "mongodb://auctionhouse-user:Test-1234@localhost:27017/appDb",
                DatabaseName = "appDb"
            });

            auction = new()
            {
                AuctionId = Guid.NewGuid().ToString(),
                Category = new CategoryRead
                {
                    Id = 1,
                    SubCategory = new CategoryRead
                    {
                        Id = 2,
                        SubCategory = new CategoryRead
                        {
                            Id = 3
                        }
                    }
                }
            };
            dbContext.AuctionsReadModel.InsertOne(auction);
            dbContext.UsersReadModel.InsertOne(new UserRead
            {
                UserIdentity = new()
                {
                    UserName = "test",
                    UserId = winnerId.ToString(),
                }
            });
            dbContext.AuctionBidsReadModel.InsertOne(new AuctionBidsRead
            {
                AuctionBidsId = auctionBidsId.ToString(),
                AuctionId = auction.AuctionId.ToString(),
            });
        }

        private IAppEvent<AuctionPriceRised> GivenAuctionPriceRisedEvent(long aggVersion = 1)
        {
            return new TestAppEventBuilder()
                .WithCommandContext(CommandContext.CreateNew("test"))
                .WithEvent(new AuctionPriceRised
                {
                    AuctionId = Guid.Parse(auction.AuctionId),
                    CategoryId = 1, SubCategoryId = 2, SubSubCategoryId = 3,
                    AggVersion = aggVersion,
                    AuctionBidsId = auctionBidsId,
                    CurrentPrice = 10,
                    BidId = bidId,
                    WinnerId = winnerId,
                    Date = DateTime.UtcNow,
                })
                .Build<AuctionPriceRised>();
        }

        [Fact]
        public async Task Consumption_of_auctionPriceRised_event_is_idempotent()
        {
            var appEvent = GivenAuctionPriceRisedEvent();
            var consumer = new AuctionPriceRisedEventConsumer(Mock.Of<ILogger<AuctionPriceRisedEventConsumer>>(), GivenMockEventConsumerDependencies(), dbContext, Mock.Of<IBidRaisedNotifications>());

            for (int i = 0; i < 2; i++)
            {
                await consumer.Consume(appEvent);

                var dbAuction = dbContext.AuctionsReadModel.Find(a => a.AuctionId == auction.AuctionId).First();

                dbAuction.ActualPrice.Should().Be(appEvent.Event.CurrentPrice); //TODO ACTUAL TO CURRENT rename
                dbAuction.WinningBid!.BidId.Should().Be(appEvent.Event.BidId.ToString());

                var dbUser = dbContext.UsersReadModel.Find(u => u.UserIdentity.UserId == winnerId.ToString()).First();

                dbUser.UserBids.Count().Should().Be(1);
                dbUser.UserBids[0].BidId.Should().Be(appEvent.Event.BidId.ToString());

                var dbAuctionBids = dbContext.AuctionBidsReadModel.Find(b => b.AuctionId == auction.AuctionId).First();

                dbAuctionBids.CurrentPrice.Should().Be(appEvent.Event.CurrentPrice);
                dbAuctionBids.Version.Should().Be(appEvent.Event.AggVersion);
            }
        }

        private static EventConsumerDependencies GivenMockEventConsumerDependencies()
        {
            return new EventConsumerDependencies(new TestAppEventBuilder(), Mock.Of<IEventConsumerCallbacks>());
        }
    }
}
