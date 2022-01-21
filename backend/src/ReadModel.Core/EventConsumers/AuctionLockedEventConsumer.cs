using Auctions.DomainEvents;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionLockedEventConsumer : EventConsumer<AuctionLocked, AuctionLockedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;

        public AuctionLockedEventConsumer(ILogger<AuctionLockedEventConsumer> logger, ReadModelDbContext dbContext, EventConsumerDependencies dependencies)
            : base(logger, dependencies)
        {
            _dbContext = dbContext;
        }

        public override async Task Consume(IAppEvent<AuctionLocked> appEvent)
        {
            var filterBuilder = Builders<AuctionRead>.Filter;
            var idFilter = filterBuilder.Eq(f => f.AuctionId, appEvent.Event.AuctionId.ToString());
            var versionFilter = filterBuilder.Lt(f => f.Version, appEvent.Event.AggVersion);
            var filter = filterBuilder.And(idFilter, versionFilter);

            var updateBuilder = Builders<AuctionRead>.Update;
            var updateLocked = updateBuilder.Set(read => read.Locked, true);
            var updateVersion = updateBuilder.Set(read => read.Version, appEvent.Event.AggVersion);
            var update = updateBuilder.Combine(updateLocked, updateVersion);

            await _dbContext.AuctionsReadModel
                .WithWriteConcern(new WriteConcern(mode: "majority", journal: true))
                .UpdateOneAsync(filter, update);
        }
    }
}
