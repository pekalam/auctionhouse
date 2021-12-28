using System;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.EventHandlers.AuctionUpdateHandlers
{
    public class AuctionImageRemovedHandler : EventConsumer<AuctionImageRemoved>
    {
        private ReadModelDbContext _dbContext;
        private readonly IRequestStatusSender _requestStatusService;
        private readonly ILogger<AuctionImageAddedHandler> _logger;

        public AuctionImageRemovedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, IRequestStatusSender requestStatusService, 
            ILogger<AuctionImageAddedHandler> logger) : base(appEventBuilder, logger)
        {
            _dbContext = dbContext;
            _requestStatusService = requestStatusService;
            _logger = logger;
        }

        private void RemoveImg(AuctionImageRemoved auctionEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, auctionEvent.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Set(read => read.AuctionImages[auctionEvent.ImgNum],
                null);
            try
            {
                _dbContext.AuctionsReadModel.UpdateMany(filter, update);
            }
            catch (Exception)
            {
                _logger.LogError("Cannot remove image in read model");
                throw;
            }
        }

        public override void Consume(IAppEvent<AuctionImageRemoved> appEvent)
        {
            RemoveImg(appEvent.Event);
            _requestStatusService.TrySendReqestCompletionToUser(appEvent, appEvent.Event.AuctionOwner);
        }
    }
}