using Auctions.DomainEvents;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionUnlockedEventConsumer : EventConsumer<AuctionUnlocked, AuctionUnlockedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;

        public AuctionUnlockedEventConsumer(ILogger<AuctionUnlockedEventConsumer> logger, ReadModelDbContext dbContext, EventConsumerDependencies dependencies)
            : base(logger, dependencies)
        {
            _dbContext = dbContext;
        }

        public override Task Consume(IAppEvent<AuctionUnlocked> appEvent)
        {
            var filterBuilder = Builders<AuctionRead>.Filter;
            var idFilter = filterBuilder.Eq(f => f.AuctionId, appEvent.Event.AuctionId.ToString());
            var versionFilter = filterBuilder.Lte(f => f.Version, appEvent.Event.AggVersion);
            var filter = filterBuilder.And(idFilter, versionFilter);

            var updateBuilder = Builders<AuctionRead>.Update;
            var updateLocked = updateBuilder.Set(read => read.Locked, false);
            var updateVersion = updateBuilder.Set(read => read.Version, appEvent.Event.AggVersion);
            var update = updateBuilder.Combine(updateLocked, updateVersion);

            _dbContext.AuctionsReadModel.UpdateMany(filter, update);
            return Task.CompletedTask;
        }
    }
}
