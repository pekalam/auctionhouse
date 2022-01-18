using Auctions.DomainEvents;
using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionImageAddedEventConsumer : EventConsumer<AuctionImageAdded, AuctionImageAddedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly ILogger<AuctionImageAddedEventConsumer> _logger;

        public AuctionImageAddedEventConsumer(ILogger<AuctionImageAddedEventConsumer> logger, EventConsumerDependencies dependencies, ReadModelDbContext dbContext) : base(logger, dependencies)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override async Task Consume(IAppEvent<AuctionImageAdded> appEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, appEvent.Event.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Set(read => read.AuctionImages[appEvent.Event.Num],
                new AuctionImageRead
                {
                    Size1Id = appEvent.Event.AddedImageSize1Id,
                    Size2Id = appEvent.Event.AddedImageSize2Id,
                    Size3Id = appEvent.Event.AddedImageSize3Id,
                });

            await _dbContext.AuctionsReadModel.UpdateManyAsync(filter, update);
        }
    }
}
