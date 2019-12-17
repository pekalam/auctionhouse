using System;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.EventHandlers
{
    public class AuctionCompletedHandler : EventConsumer<AuctionCompleted>
    {
        private ReadModelDbContext _dbContext;
        private readonly IRequestStatusService _requestStatusService;

        public AuctionCompletedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, IRequestStatusService requestStatusService) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _requestStatusService = requestStatusService;
        }

        private void UserBidsUpdate(IClientSessionHandle session, AuctionCompleted ev)
        {
            var bidFilter = Builders<UserBid>.Filter.Eq(f => f.AuctionId, ev.AuctionId.ToString());
            var filter = Builders<UserRead>.Filter.ElemMatch(model => model.UserBids, bidFilter);
            var update = Builders<UserRead>.Update.Set(f => f.UserBids[-1].AuctionCompleted, true);

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

            var auctionFilter = Builders<AuctionRead>.Filter.Eq(field => field.AuctionId, ev.AuctionId.ToString());
            var auctionUpdate = Builders<AuctionRead>.Update
                .Set(field => field.Completed, true)
                .Set(field => field.Buyer, new UserIdentityRead(ev.WinningBid?.UserIdentity))
                .Set(field => field.WinningBid, new BidRead(ev.WinningBid));

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