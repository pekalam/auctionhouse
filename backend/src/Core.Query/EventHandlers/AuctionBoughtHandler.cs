using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Query.Exceptions;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.EventHandlers
{
    public class AuctionBoughtHandler : EventConsumer<AuctionBought>
    {
        private ReadModelDbContext _readModelDbContext;
        private ILogger<AuctionBoughtHandler> _logger;

        public AuctionBoughtHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext readModelDbContext, ILogger<AuctionBoughtHandler> logger) : base(appEventBuilder, logger)
        {
            _readModelDbContext = readModelDbContext;
            _logger = logger;
        }

        private void UpdateUser(IClientSessionHandle session, AuctionBought ev)
        {
            var filter =
                Builders<UserRead>.Filter.Eq(read => read.UserIdentity.UserId, ev.UserIdentity.UserId.ToString());

            var upd1 = Builders<UserRead>.Update.Push(read => read.BoughtAuctions, ev.AuctionId.ToString());

            var result = _readModelDbContext.UsersReadModel.UpdateMany(session, filter, upd1);
            if (result.ModifiedCount == 0)
            {
                throw new QueryException("Cannot update UserReadModel (ModifiedCount = 0)");
            }
        }

        private void UpdateAuction(IClientSessionHandle session, AuctionBought ev)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(read => read.AuctionId, ev.AuctionId.ToString());
            var upd1 = Builders<AuctionRead>.Update.Set(read => read.Bought, true);
            var upd2 = Builders<AuctionRead>.Update.Set(read => read.Buyer, new UserIdentityRead(ev.UserIdentity));
            var upd3 = Builders<AuctionRead>.Update.Set(read => read.Archived, true);

            var result = _readModelDbContext.AuctionsReadModel.UpdateMany(session, filter,
                Builders<AuctionRead>.Update.Combine(upd1, upd2, upd3));
            if (result.ModifiedCount == 0)
            {
                throw new QueryException("Cannot update AuctionReadModel (ModifiedCount = 0)");
            }
        }

        public override void Consume(IAppEvent<AuctionBought> appEvent)
        {
            var ev = appEvent.Event;


            var session = _readModelDbContext.Client.StartSession();
            session.StartTransaction();
            try
            {
                UpdateAuction(session, ev);
                UpdateUser(session, ev);
                session.CommitTransaction();
            }
            catch (Exception e)
            {
                session.AbortTransaction();
                _logger.LogError(e, "Cannot update read collections, appEvent: {@appEvent}", appEvent);
                throw;
            }
        }
    }
}