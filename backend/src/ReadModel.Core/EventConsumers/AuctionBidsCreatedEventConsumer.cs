using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
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
            };

            await _dbContext.AuctionBidsReadModel.InsertOneAsync(auctionBidsRead);
        }
    }
}
