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

        public AuctionLockedEventConsumer(IAppEventBuilder appEventBuilder, ILogger<AuctionLockedEventConsumer> logger,
            Lazy<ISagaNotifications> sagaNotificationsFactory, Lazy<IImmediateNotifications> immediateNotifications, ReadModelDbContext dbContext)
            : base(appEventBuilder, logger, sagaNotificationsFactory, immediateNotifications)
        {
            _dbContext = dbContext;
        }

        public override void Consume(IAppEvent<AuctionLocked> appEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, appEvent.Event.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Set(read => read.Locked, true);

            _dbContext.AuctionsReadModel.UpdateMany(filter, update);
        }
    }

    public class AuctionUnlockedEventConsumer : EventConsumer<AuctionUnlocked, AuctionUnlockedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;

        public AuctionUnlockedEventConsumer(IAppEventBuilder appEventBuilder, ILogger<AuctionUnlockedEventConsumer> logger,
            Lazy<ISagaNotifications> sagaNotificationsFactory, Lazy<IImmediateNotifications> immediateNotifications, ReadModelDbContext dbContext)
            : base(appEventBuilder, logger, sagaNotificationsFactory, immediateNotifications)
        {
            _dbContext = dbContext;
        }

        public override void Consume(IAppEvent<AuctionUnlocked> appEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, appEvent.Event.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Set(read => read.Locked, false);

            _dbContext.AuctionsReadModel.UpdateMany(filter, update);
        }
    }
}
