using Auctions.DomainEvents;
using AutoFixture;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using ReadModel.Core.EventConsumers;
using ReadModel.Core.Model;
using System;
using System.Collections.Generic;
using Common.Tests.Base.Mocks.Events;
using Xunit;

namespace Test.ReadModel.Integration
{
    [Trait("Category", "Integration")]
    public class AuctionLockingEventConsumer_Tests : IDisposable
    {
        AuctionRead? insertedAuctionRead;
        ReadModelDbContext dbContext;

        public AuctionLockingEventConsumer_Tests()
        {
            dbContext = new ReadModelDbContext(
                new MongoDbSettings
                {
                    ConnectionString = "mongodb://localhost:27017",
                    DatabaseName = "appDb"
                });
        }

        public void Dispose()
        {
            if (insertedAuctionRead == null) return;
            dbContext.AuctionsReadModel.DeleteOne(Builders<AuctionRead>.Filter.Eq(a => a.Id, insertedAuctionRead.Id));
        }

        [Fact]
        public void AuctionLockedEventConsumer_should_update_auction_when_AggVersion_greater_than_to_inserted()
        {
            var auctionRead = GivenAuctionRead();
            auctionRead.Version = 1;
            var auctionLockedConsumer = GivenAuctionLockedConsumer();

            //setup AuctionRead
            dbContext.AuctionsReadModel.InsertOne(auctionRead);
            ConsumeAuctionEvent(GivenAuctionLockedEvent(2, auctionRead), auctionLockedConsumer);

            var dbAuctionReadModels = FindInsertedAuctionRead();
            dbAuctionReadModels[0].Locked.Should().BeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void AuctionLockedEventConsumer_should_not_update_auction_when_AggVersion_less_or_equal_to_inserted(long aggVersion)
        {
            var auctionRead = GivenAuctionRead();
            auctionRead.Version = 2;
            var auctionLockedConsumer = GivenAuctionLockedConsumer();

            //setup AuctionRead
            dbContext.AuctionsReadModel.InsertOne(auctionRead);
            ConsumeAuctionEvent(GivenAuctionLockedEvent(aggVersion, auctionRead), auctionLockedConsumer);

            var dbAuctionReadModels = FindInsertedAuctionRead();
            dbAuctionReadModels[0].Locked.Should().BeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void AuctionUnlockedEventConsumer_should_not_update_auction_when_AggVersion_less_or_eq_to_inserted(long aggVersion)
        {
            var auctionRead = GivenAuctionRead();
            auctionRead.Version = 2;
            auctionRead.Locked = true;
            var auctionUnlockedConsumer = GivenAuctionUnlockedConsumer();

            //setup AuctionRead
            dbContext.AuctionsReadModel.InsertOne(auctionRead);
            ConsumeAuctionEvent(GivenAuctionUnlockedEvent(aggVersion, auctionRead), auctionUnlockedConsumer);

            var dbAuctionReadModels = FindInsertedAuctionRead();
            dbAuctionReadModels[0].Locked.Should().BeTrue();
        }

        [Fact]
        public void AuctionUnlockedEventConsumer_should_update_auction_when_AggVersion_greater_than_inserted()
        {
            var auctionRead = GivenAuctionRead();
            auctionRead.Version = 1;
            auctionRead.Locked = true;
            var auctionUnlockedConsumer = GivenAuctionUnlockedConsumer();

            //setup AuctionRead
            dbContext.AuctionsReadModel.InsertOne(auctionRead);
            ConsumeAuctionEvent(GivenAuctionUnlockedEvent(2, auctionRead), auctionUnlockedConsumer);

            var dbAuctionReadModels = FindInsertedAuctionRead();
            dbAuctionReadModels[0].Locked.Should().BeFalse();
        }

        private AuctionLockedEventConsumer GivenAuctionLockedConsumer()
        {
            return new AuctionLockedEventConsumer(Mock.Of<ILogger<AuctionLockedEventConsumer>>(), dbContext, GivenMockEventConsumerDependencies());
        }

        private AuctionUnlockedEventConsumer GivenAuctionUnlockedConsumer()
        {
            return new AuctionUnlockedEventConsumer(Mock.Of<ILogger<AuctionUnlockedEventConsumer>>(), dbContext, GivenMockEventConsumerDependencies());
        }

        private static EventConsumerDependencies GivenMockEventConsumerDependencies()
        {
            return new EventConsumerDependencies(new TestAppEventBuilder(), 
                new(() => Mock.Of<ISagaNotifications>()), new(() => Mock.Of<IImmediateNotifications>()));
        }

        private static void ConsumeAuctionEvent<T>(T @event, IEventDispatcher eventDispatcher) where T : Event
        {
            eventDispatcher.Dispatch(new TestAppEvent<T>
            {
                CommandContext = CommandContext.CreateNew("test"),
                Event = @event,
                ReadModelNotifications = ReadModelNotificationsMode.Disabled,
            }).GetAwaiter().GetResult();
        }

        private static AuctionLocked GivenAuctionLockedEvent(long aggVersion, AuctionRead auctionRead)
        {
            return new AuctionLocked
            {
                AuctionId = Guid.Parse(auctionRead.AuctionId),
                LockIssuer = Guid.NewGuid(),
                AggVersion = aggVersion,
            };
        }

        private static AuctionUnlocked GivenAuctionUnlockedEvent(long aggVersion, AuctionRead auctionRead)
        {
            return new AuctionUnlocked
            {
                AuctionId = Guid.Parse(auctionRead.AuctionId),
                AggVersion = aggVersion,
            };
        }

        private List<AuctionRead> FindInsertedAuctionRead()
        {
            return dbContext.AuctionsReadModel
                .Find(Builders<AuctionRead>.Filter.Eq(a => a.Id, insertedAuctionRead!.Id))
                .ToList();
        }


        private AuctionRead GivenAuctionRead()
        {
            var fixture = new Fixture();
            fixture.Customize<BidRead>(opt => opt.Without(b => b.Id));
            fixture.Customize<AuctionRead>(opt => opt
                .Without(a => a.Id)
                .Without(a => a.Category)
                .Without(a => a.Locked)
                .With(a => a.AuctionId, () => Guid.NewGuid().ToString()));
            var auctionRead = insertedAuctionRead = fixture.Create<AuctionRead>();
            return auctionRead;
        }
    }
}