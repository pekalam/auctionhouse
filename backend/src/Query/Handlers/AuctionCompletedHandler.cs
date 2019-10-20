using System;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.Handlers
{
    public class AuctionCompletedHandler : EventConsumer<AuctionCompleted>
    {
        private ReadModelDbContext _dbContext;
        private readonly IEventSignalingService _eventSignalingService;

        public AuctionCompletedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, IEventSignalingService eventSignalingService) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _eventSignalingService = eventSignalingService;
        }

        private void UserBidsUpdate(IClientSessionHandle session, AuctionCompleted ev)
        {
            var bidFilter = Builders<UserBid>.Filter.Eq(f => f.AuctionId, ev.AuctionId.ToString());
            var filter = Builders<UserReadModel>.Filter.ElemMatch(model => model.UserBids, bidFilter);
            var update = Builders<UserReadModel>.Update.Set(f => f.UserBids[-1].AuctionCompleted, true);

            try
            {
                _dbContext.UsersReadModel.UpdateMany(filter, update);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void Consume(IAppEvent<AuctionCompleted> message)
        {
            AuctionCompleted ev = message.Event;

            var auctionFilter = Builders<AuctionReadModel>.Filter.Eq(field => field.AuctionId, ev.AuctionId.ToString());
            var auctionUpdate = Builders<AuctionReadModel>.Update
                .Set(field => field.Completed, true)
                .Set(field => field.Buyer, ev.WinningBid?.UserIdentity)
                .Set(field => field.WinningBid, ev.WinningBid);

            var session = _dbContext.Client.StartSession();
            session.StartTransaction();
            try
            {
                _dbContext.AuctionsReadModel.UpdateMany(session, auctionFilter, auctionUpdate);
                UserBidsUpdate(session, ev);
                session.CommitTransaction();
            }
            catch (Exception)
            {
                session.AbortTransaction();
                throw;
            }
        }
    }
}