using System;
using System.Linq;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Core.Query.Handlers
{
    public class AuctionCreatedHandler : EventConsumer<AuctionCreated>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IEventSignalingService _eventSignalingService;

        public AuctionCreatedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, IEventSignalingService eventSignalingService) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _eventSignalingService = eventSignalingService;
        }

        public override void Consume(IAppEvent<AuctionCreated> message)
        {
            var ev = message.Event;
            var auction = new AuctionReadModel()
            {
                Id = ObjectId.GenerateNewId(),
                Product = ev.AuctionArgs.Product,
                BuyNowPrice = ev.AuctionArgs.BuyNowPrice,
                BuyNowOnly = ev.AuctionArgs.BuyNowOnly,
                StartDate = ev.AuctionArgs.StartDate,
                EndDate = ev.AuctionArgs.EndDate,
                Creator = ev.AuctionArgs.Creator,
                AuctionId = ev.AuctionId.ToString(),
                Category = ev.AuctionArgs.Category,
                Version =  ev.AggVersion,
                AuctionImages = ev.AuctionArgs.AuctionImages,
                Tags = ev.AuctionArgs.Tags
            };

            var filter = Builders<UserReadModel>.Filter.Eq(f => f.UserIdentity.UserId, ev.AuctionArgs.Creator.UserId.ToString());
            var update = Builders<UserReadModel>.Update.Push(f => f.CreatedAuctions, ev.AuctionId.ToString());


            var session = _dbContext.Client.StartSession();

            session.StartTransaction();
            try
            {
                _dbContext.AuctionsReadModel.WithWriteConcern(WriteConcern.WMajority).InsertOne(session, auction);
                _dbContext.UsersReadModel.UpdateOne(session, filter, update);
                session.CommitTransaction();
            }
            catch (Exception)
            {
                session.AbortTransaction();
                _eventSignalingService.TrySendEventFailureToUser(message, ev.AuctionArgs.Creator);
                throw;
            }

            _eventSignalingService.TrySendEventCompletionToUser(message, ev.AuctionArgs.Creator);
        }
    }
}
