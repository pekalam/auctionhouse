using System;
using System.Linq;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.EventHandlers
{
    public class BidCanceledHandler : EventConsumer<BidCanceled>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly ILogger<BidCanceledHandler> _logger;

        public BidCanceledHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            ILogger<BidCanceledHandler> logger) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private void UpdateAuction(IClientSessionHandle session, BidCanceled bidCanceled)
        {
            var filter =
                Builders<AuctionRead>.Filter.Eq(read => read.AuctionId, bidCanceled.CanceledBid.AuctionId.ToString());
            var updateBuilder = Builders<AuctionRead>.Update;

            UpdateDefinition<AuctionRead> update;
            var verUpd = updateBuilder.Set(read => read.Version, bidCanceled.AggVersion);
            if (bidCanceled.NewWinner != null && (bidCanceled.CanceledBid.BidId == bidCanceled.NewWinner.BidId))
            {
                var upd1 = updateBuilder.Set(read => read.ActualPrice, 0);
                var upd2 = updateBuilder.Set(read => read.WinningBid, null);
                update = updateBuilder.Combine(upd1, upd2, verUpd);
            }
            else if (bidCanceled.NewWinner != null)
            {
                var upd1 = updateBuilder.Set(read => read.ActualPrice, bidCanceled.NewWinner.Price);
                var upd2 = updateBuilder.Set(read => read.WinningBid, new BidRead(bidCanceled.NewWinner));
                update = updateBuilder.Combine(upd1, upd2, verUpd);
            }
            else
            {
                update = verUpd;
            }

            _dbContext.AuctionsReadModel.UpdateMany(session, filter, update);
        }

        private void UpdateUser(IClientSessionHandle session, BidCanceled bidCanceled)
        {
            var filter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId,
                bidCanceled.CanceledBid.UserIdentity.UserId.ToString());

            var bidFilter =
                Builders<UserBid>.Filter.Eq(userBid => userBid.BidId, bidCanceled.CanceledBid.BidId.ToString());
            var elFilter = Builders<UserRead>.Filter.ElemMatch(read => read.UserBids, bidFilter);

            var bidUpd = Builders<UserRead>.Update.Set(read => read.UserBids[-1].BidCanceled, true);


            var result = _dbContext.UsersReadModel
                .UpdateMany(Builders<UserRead>.Filter.And(filter, elFilter), bidUpd);

            if (result.ModifiedCount == 0)
            {
                throw new Exception($"Cannot update user bid {bidCanceled.CanceledBid.BidId}");
            }
        }

        public override void Consume(IAppEvent<BidCanceled> appEvent)
        {
            var session = _dbContext.Client.StartSession();
            session.StartTransaction();
            try
            {
                UpdateAuction(session, appEvent.Event);
                UpdateUser(session, appEvent.Event);
                session.CommitTransaction();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot cancel bid, appEvent: {@appEvent}", appEvent);
                throw;
            }
        }
    }
}