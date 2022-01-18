using Auctions.DomainEvents;
using Common.Application.Events;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace Core.Query.EventHandlers.AuctionUpdateHandlers
{
    public class AuctionImageReplacedEventConsumer : EventConsumer<AuctionImageReplaced, AuctionImageReplacedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly ILogger<AuctionImageReplacedEventConsumer> _logger;

        public AuctionImageReplacedEventConsumer(ILogger<AuctionImageReplacedEventConsumer> logger, 
            EventConsumerDependencies dependencies, ReadModelDbContext dbContext) : base(logger, dependencies)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override async Task Consume(IAppEvent<AuctionImageReplaced> appEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, appEvent.Event.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Set(read => read.AuctionImages[appEvent.Event.ImgNum],
                new AuctionImageRead
                {
                    Size1Id = appEvent.Event.ImageSize1Id,
                    Size2Id = appEvent.Event.ImageSize2Id,
                    Size3Id = appEvent.Event.ImageSize3Id,
                });
            await _dbContext.AuctionsReadModel.UpdateManyAsync(filter, update);
        }
    }
}