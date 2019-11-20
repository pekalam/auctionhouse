using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Auctions.Events.Update;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReflectionMagic;

namespace Core.Query.Handlers.AuctionUpdateHandlers
{
    public class AuctionUpdatedHandler : EventConsumer<AuctionUpdateEventGroup>
    {
        private ReadModelDbContext _dbContext;
        private readonly IEventSignalingService _eventSignalingService;
        private readonly ILogger<AuctionUpdatedHandler> _logger;

        public AuctionUpdatedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            IEventSignalingService eventSignalingService, ILogger<AuctionUpdatedHandler> logger) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _eventSignalingService = eventSignalingService;
            _logger = logger;
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionBuyNowPriceChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.BuyNowPrice, ev.BuyNowPrice.Value);
        }

        public override void Consume(IAppEvent<AuctionUpdateEventGroup> appEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, appEvent.Event.AggregateId.ToString());
            var updates = new List<UpdateDefinition<AuctionRead>>();
            appEvent.Event.UpdateEvents.ForEach(ev => updates.Add(this.AsDynamic().UpdateAuction(appEvent.Event, ev)));

            var update = Builders<AuctionRead>.Update.Combine(updates.Distinct().AsEnumerable());

            try
            {
                _dbContext.AuctionsReadModel.FindOneAndUpdate(filter, update);
                _eventSignalingService.TrySendEventCompletionToUser(appEvent, appEvent.Event.AuctionOwner);
            }
            catch (Exception)
            {
                _logger.LogError("Cannot add image to read model");
                _eventSignalingService.TrySendEventFailureToUser(appEvent, appEvent.Event.AuctionOwner);
                throw;
            }
        }
    }
}