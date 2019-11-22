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

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionCategoryChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.Category, ev.Category);
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionEndDateChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.EndDate, ev.Date.Value);
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionNameChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.Name, ev.AuctionName.Value);
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionTagsChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.Tags, ev.Tags.Select(tag => tag.Value));
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionDescriptionChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.Product.Description, ev.Description);
        }

        public override void Consume(IAppEvent<AuctionUpdateEventGroup> appEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, appEvent.Event.AggregateId.ToString());
            var updates = new List<UpdateDefinition<AuctionRead>>();
            appEvent.Event.UpdateEvents.ForEach(ev =>
                updates.Add((UpdateDefinition<AuctionRead>) this.AsDynamic().UpdateAuction(appEvent.Event, ev)));

            var update = Builders<AuctionRead>.Update.Combine(updates);

            try
            {
                _dbContext.AuctionsReadModel.UpdateMany(filter, update);
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