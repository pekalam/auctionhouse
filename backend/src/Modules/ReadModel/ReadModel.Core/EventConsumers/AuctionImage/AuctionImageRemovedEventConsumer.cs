using Auctions.DomainEvents;
using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionImageRemovedEventConsumer : EventConsumer<AuctionImageRemoved, AuctionImageRemovedEventConsumer>
    {
        private ReadModelDbContext _dbContext;
        private readonly ILogger<AuctionImageRemovedEventConsumer> _logger;

        public AuctionImageRemovedEventConsumer(ILogger<AuctionImageRemovedEventConsumer> logger, EventConsumerDependencies dependencies, ReadModelDbContext dbContext) : base(logger, dependencies)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override async Task Consume(IAppEvent<AuctionImageRemoved> appEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, appEvent.Event.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Set(read => read.AuctionImages[appEvent.Event.ImgNum], null);

            await _dbContext.AuctionsReadModel.UpdateManyAsync(filter, update);
        }
    }
}