using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Core.Query.Exceptions;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.EventHandlers
{
    public class AuctionBoughtHandler : EventConsumer<AuctionBought>
    {
        private ReadModelDbContext _readModelDbContext;
        private readonly IRequestStatusSender _requestStatusService;
        private ILogger<AuctionBoughtHandler> _logger;

        public AuctionBoughtHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext readModelDbContext,
            IRequestStatusSender requestStatusService, ILogger<AuctionBoughtHandler> logger) : base(appEventBuilder,
            logger)
        {
            _readModelDbContext = readModelDbContext;
            _requestStatusService = requestStatusService;
            _logger = logger;
        }

        private void UpdateBids(IClientSessionHandle session, AuctionBought ev)
        {
            var bidFilter = Builders<UserBid>.Filter.Eq(f => f.AuctionId, ev.AuctionId.ToString());
            var filter = Builders<UserRead>.Filter.ElemMatch(model => model.UserBids, bidFilter);
            var update = Builders<UserRead>.Update.Set(f => f.UserBids[-1].AuctionCompleted, true);

            try
            {
                _readModelDbContext.UsersReadModel.UpdateMany(session, filter, update);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateAuction(IClientSessionHandle session, AuctionBought ev)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(read => read.AuctionId, ev.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update
                .Set(read => read.Bought, true)
                .Set(read => read.Completed, true)
                .Set(read => read.Buyer, new UserIdentityRead(ev.UserIdentity, ev.UserIdentity.ToString())) //TODO
                .Set(read => read.Archived, true);

            var result = _readModelDbContext.AuctionsReadModel.UpdateMany(session, filter, update);
            if (result.ModifiedCount == 0)
            {
                throw new QueryException("Cannot update AuctionReadModel (ModifiedCount = 0)");
            }
        }

        public override void Consume(IAppEvent<AuctionBought> appEvent)
        {
            var ev = appEvent.Event;

            using (var session = _readModelDbContext.Client.StartSession())
            {
                try
                {
                    _ = session.WithTransaction((handle, token) =>
                    {
                        UpdateAuction(handle, ev);
                        UpdateBids(handle, ev);
                        return 0;
                    });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Cannot update read collections, appEvent: {@appEvent}", appEvent);
                    _requestStatusService.TrySendRequestFailureToUser(appEvent, ev.UserIdentity);
                    throw;
                }
                _requestStatusService.TrySendReqestCompletionToUser(appEvent, ev.UserIdentity);
            }
        }
    }
}