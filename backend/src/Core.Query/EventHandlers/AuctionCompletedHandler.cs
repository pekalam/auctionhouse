using System;
using Core.Common.Domain.AuctionBids;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Query.Exceptions;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.EventHandlers
{
    public class AuctionCompletedHandler : EventConsumer<AuctionCompleted>
    {
        private ReadModelDbContext _dbContext;

        public AuctionCompletedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, ILogger<AuctionCompletedHandler> logger) : base(appEventBuilder, logger)
        {
            _dbContext = dbContext;
        }

        private void UserBidsUpdate(IClientSessionHandle session, AuctionCompleted ev)
        {
            var bidFilter = Builders<UserBid>.Filter.Eq(f => f.AuctionId, ev.AuctionId.ToString());
            var filter = Builders<UserRead>.Filter.ElemMatch(model => model.UserBids, bidFilter);
            var update = Builders<UserRead>.Update.Set(f => f.UserBids[-1].AuctionCompleted, true);

            try
            {
                _dbContext.UsersReadModel.UpdateMany(session, filter, update);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateAuction(IClientSessionHandle session, AuctionCompleted ev)
        {
            var auctionFilter = Builders<AuctionRead>.Filter.Eq(field => field.AuctionId, ev.AuctionId.ToString());
            var auctionUpdate = Builders<AuctionRead>.Update
                .Set(field => field.Completed, true)
                .Set(field => field.Buyer, new UserIdentityRead(ev.WinnerId, ev.WinnerId.ToString())) //TODO
                //.Set(field => field.WinningBid, new BidRead(ev.AuctionBidsId)) //TODO
                .Set(read => read.Archived, true);

            _dbContext.AuctionsReadModel.UpdateMany(session, auctionFilter, auctionUpdate);
        }

        public override void Consume(IAppEvent<AuctionCompleted> message)
        {
            AuctionCompleted ev = message.Event;

            using (var session = _dbContext.Client.StartSession())
            {
                var opt = new TransactionOptions(
                    writeConcern: new WriteConcern(mode: "majority", journal: true)
                );

                _ = session.WithTransaction((handle, token) =>
                {
                    UpdateAuction(handle, ev);
                    UserBidsUpdate(handle, ev);
                    return 0;
                }, opt);
            }
        }
    }
}