using System;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.Handlers.AuctionUpdateHandlers
{
    public class AuctionImageAddedHandler : EventConsumer<AuctionImageAdded>
    {
        private ReadModelDbContext _dbContext;
        private readonly IEventSignalingService _eventSignalingService;
        private readonly ILogger<AuctionImageAddedHandler> _logger;

        public AuctionImageAddedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, IEventSignalingService eventSignalingService, ILogger<AuctionImageAddedHandler> logger) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _eventSignalingService = eventSignalingService;
            _logger = logger;
        }

        private void AddImg(AuctionImageAdded auctionEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, auctionEvent.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Set(read => read.AuctionImages[auctionEvent.Num],
                auctionEvent.AddedImage);
            try
            {
                _dbContext.AuctionsReadModel.UpdateMany(filter, update);
            }
            catch (Exception)
            {
                _logger.LogError("Cannot add image to read model");
                throw;
            }
        }

        public override void Consume(IAppEvent<AuctionImageAdded> appEvent)
        {
            AddImg(appEvent.Event);
            

            _eventSignalingService.TrySendEventCompletionToUser(appEvent, appEvent.Event.AuctionOwner);
        }
    }
}
