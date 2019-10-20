using System;
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
                Product = ev.Product,
                BuyNowPrice = ev.BuyNowPrice,
                StartDate = ev.StartDate,
                EndDate = ev.EndDate,
                Creator = new UserIdentityReadModel(ev.Creator),
                AuctionId = ev.AuctionId.ToString(),
                Category = ev.Category,
                Version =  ev.AggVersion,
                AuctionImages = ev.AuctionImages
            };

            var filter = Builders<UserReadModel>.Filter.Eq(f => f.UserIdentity.UserId, ev.Creator.UserId.ToString());
            var update = Builders<UserReadModel>.Update.Push(f => f.CreatedAuctions, ev.AuctionId.ToString());


            var session = _dbContext.Client.StartSession();

            session.StartTransaction();
            try
            {
                _dbContext.AuctionsReadModel.WithWriteConcern(WriteConcern.WMajority).InsertOne(session,auction);
                _dbContext.UsersReadModel.FindOneAndUpdate(session, filter, update);
                session.CommitTransaction();
            }
            catch (Exception)
            {
                session.AbortTransaction();
                _eventSignalingService.TrySendEventFailureToUser(message, message.Event.Creator);
                throw;
            }

            _eventSignalingService.TrySendEventCompletionToUser(message, message.Event.Creator);
        }
    }
}
