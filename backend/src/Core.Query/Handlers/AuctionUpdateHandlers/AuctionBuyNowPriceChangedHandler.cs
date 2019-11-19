using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.Handlers.AuctionUpdateHandlers
{
    public class AuctionBuyNowPriceChangedHandler : EventConsumer<AuctionBuyNowPriceChanged>
    {
        private ReadModelDbContext _dbContext;
        private readonly IEventSignalingService _eventSignalingService;
        private readonly ILogger<AuctionBuyNowPriceChangedHandler> _logger;

        public AuctionBuyNowPriceChangedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, IEventSignalingService eventSignalingService, ILogger<AuctionBuyNowPriceChangedHandler> logger) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _eventSignalingService = eventSignalingService;
            _logger = logger;
        }

        public override void Consume(IAppEvent<AuctionBuyNowPriceChanged> appEvent)
        {
            var ev = appEvent.Event;
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, ev.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Set(read => read.BuyNowPrice, ev.BuyNowPrice.Value);
            try
            {
                _dbContext.AuctionsReadModel.FindOneAndUpdate(filter, update);
            }
            catch (Exception)
            {
                _logger.LogError("Cannot add image to read model");
                _eventSignalingService.TrySendEventFailureToUser(appEvent, ev.Owner);
                throw;
            }
        }


    }
}
