using Auctions.DomainEvents;
using Auctions.DomainEvents.Update;
using Common.Application.Events;
using Core.Common.Domain.Categories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Core.EventConsumers;
using ReadModel.Core.Model;
using ReflectionMagic;

namespace Core.Query.EventHandlers.AuctionUpdateHandlers
{
    public class AuctionUpdatedEventConsumer : EventConsumer<AuctionUpdateEventGroup, AuctionUpdatedEventConsumer>
    {
        private ReadModelDbContext _dbContext;
        private readonly ILogger<AuctionUpdatedEventConsumer> _logger;
        private readonly CategoryBuilder _categoryBuilder;


        public AuctionUpdatedEventConsumer(ILogger<AuctionUpdatedEventConsumer> logger, ReadModelDbContext dbContext, 
            CategoryBuilder categoryBuilder, EventConsumerDependencies dependencies) : base(logger, dependencies)
        {
            _dbContext = dbContext;
            _logger = logger;
            _categoryBuilder = categoryBuilder;
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionBuyNowPriceChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.BuyNowPrice, ev.BuyNowPrice);
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionCategoriesChanged ev)
        {
            var category = _categoryBuilder.FromCategoryIdList(ev.Categories.ToList());
            if (category is null)
            {
                throw new NullReferenceException("Invalid categories for auctionCategoriesChanged event");
            }

            var categoryRead = CategoryRead.FromCategory(category);

            return Builders<AuctionRead>.Update.Set(read => read.Category, categoryRead);
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionEndDateChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.EndDate, ev.Date);
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionNameChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.Name, ev.AuctionName);
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionTagsChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.Tags, ev.Tags.Select(tag => tag).ToArray());
        }

        private UpdateDefinition<AuctionRead> UpdateAuction(AuctionUpdateEventGroup eventGroup,
            AuctionDescriptionChanged ev)
        {
            return Builders<AuctionRead>.Update.Set(read => read.Product.Description, ev.Description);
        }

        public override async Task Consume(IAppEvent<AuctionUpdateEventGroup> appEvent)
        {
            var idFilter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, appEvent.Event.AggregateId.ToString());
            var filter = Builders<AuctionRead>.Filter.And(CategoryFilterFactory.Create(appEvent.Event.CategoryIds), idFilter);
            var updates = new List<UpdateDefinition<AuctionRead>>();
            appEvent.Event.UpdateEvents.ForEach(ev =>
                updates.Add((UpdateDefinition<AuctionRead>) this.AsDynamic().UpdateAuction(appEvent.Event, ev)));

            var update = Builders<AuctionRead>.Update.Combine(updates);

            await _dbContext.AuctionsReadModel.UpdateOneAsync(filter, update);
        }
    }
}