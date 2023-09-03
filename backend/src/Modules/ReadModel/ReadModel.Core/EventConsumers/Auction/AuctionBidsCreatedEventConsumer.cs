using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Core.Model;
using static AuctionBids.DomainEvents.Events.V1;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionBidsCreatedEventConsumer : EventConsumer<AuctionBidsCreated, AuctionBidsCreatedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;


        public AuctionBidsCreatedEventConsumer(ILogger<AuctionBidsCreatedEventConsumer> logger,
            EventConsumerDependencies dependencies, ReadModelDbContext dbContext)
            : base(logger, dependencies)
        {
            _dbContext = dbContext;
        }

        public override async Task Consume(IAppEvent<AuctionBidsCreated> appEvent)
        {
            var auctionBidsRead = new AuctionBidsRead
            {
                AuctionId = appEvent.Event.AuctionId.ToString(),
                OwnerId = appEvent.Event.OwnerId.ToString(),
                AuctionBidsId = appEvent.Event.AuctionBidsId.ToString(),
            };

            var exists = (await _dbContext.AuctionBidsReadModel.Find(m => m.AuctionId == auctionBidsRead.AuctionId).FirstOrDefaultAsync()) != null;
            if (exists)
            {
                return;
            }

            await _dbContext.AuctionBidsReadModel.InsertOneAsync(auctionBidsRead);
        }
    }
}
