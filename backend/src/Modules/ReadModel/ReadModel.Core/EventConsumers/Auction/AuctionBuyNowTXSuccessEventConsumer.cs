using Auctions.DomainEvents;
using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionBuyNowTXSuccessEventConsumer : EventConsumer<Events.V1.BuyNowTXSuccess, AuctionBuyNowTXSuccessEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;

        public AuctionBuyNowTXSuccessEventConsumer(ILogger<AuctionBuyNowTXSuccessEventConsumer> logger, EventConsumerDependencies dependencies, ReadModelDbContext dbContext) : base(logger, dependencies)
        {
            _dbContext = dbContext;
        }

        public override async Task Consume(IAppEvent<Events.V1.BuyNowTXSuccess> appEvent)
        {
            var buyerName = await _dbContext.UsersReadModel
                .Find(u => u.UserIdentity.UserId == appEvent.Event.BuyerId.ToString())
                .Project(u => u.UserIdentity.UserName)
                .SingleAsync();

            var catFilter = CategoryFilterFactory.Create(appEvent.Event.CategoryIds);
            var idFilter = Builders<AuctionRead>.Filter.Eq(a => a.AuctionId,
                appEvent.Event.AuctionId.ToString());
            var filter = Builders<AuctionRead>.Filter.And(catFilter, idFilter);

            var update = Builders<AuctionRead>.Update
                .Set(a => a.Buyer, new UserIdentityRead
                {
                    UserId = appEvent.Event.BuyerId.ToString(),
                    UserName = buyerName,
                })
                .Set(a => a.Completed, true)
                .Set(a => a.Bought, true)
                .Set(a => a.Archived, true)
                .Set(a => a.EndDate, appEvent.Event.EndDate);

            _dbContext.AuctionsReadModel.UpdateOne(filter, update);
        }
    }
}
