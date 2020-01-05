using System;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using Core.Query.Exceptions;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.EventHandlers
{
    public class AuctionRaisedHandler : EventConsumer<AuctionRaised>
    {
        private ReadModelDbContext _dbContext;
        private readonly IRequestStatusService _requestStatusService;
        private readonly ILogger<AuctionRaisedHandler> _logger;

        public AuctionRaisedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            IRequestStatusService requestStatusService, ILogger<AuctionRaisedHandler> logger) : base(appEventBuilder,
            logger)
        {
            _dbContext = dbContext;
            _requestStatusService = requestStatusService;
            _logger = logger;
        }

        private void UpdateAuction(AuctionRaised ev, IClientSessionHandle session)
        {
            var auctionIdFilter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, ev.Bid.AuctionId.ToString());

            //bid date filter instead of version to minimize potential concurrency problems caused by command handler
            var bidDateFilter = Builders<AuctionRead>.Filter.Lt(f => f.WinningBid.DateCreated, ev.Bid.DateCreated);
            var emptyBidFilter = Builders<AuctionRead>.Filter.Eq(f => f.WinningBid, null);
            var byBidFilter = Builders<AuctionRead>.Filter.Or(new[] {emptyBidFilter, bidDateFilter});

            var bidUpdate = Builders<AuctionRead>.Update.Set(f => f.WinningBid, new BidRead(ev.Bid));
            var priceUpd = Builders<AuctionRead>.Update.Set(f => f.ActualPrice, ev.Bid.Price);
            var totalBidsUpd = Builders<AuctionRead>.Update.Inc(f => f.TotalBids, 1);
            var versionUpd = Builders<AuctionRead>.Update.Set(f => f.Version, ev.AggVersion);

            var result = _dbContext.AuctionsReadModel.UpdateMany(session,
                Builders<AuctionRead>.Filter.And(new[] {auctionIdFilter, byBidFilter}),
                Builders<AuctionRead>.Update.Combine(new[] {bidUpdate, totalBidsUpd, priceUpd, versionUpd}));
            if (result.MatchedCount == 0)
            {
                throw new QueryException($"Cannot find auction with id: {ev.Bid.AuctionId} and" +
                                         $"Version: {ev.AggVersion}");
            }
        }

        private void UpdateUser(AuctionRaised ev, IClientSessionHandle session)
        {
            var userBid = new UserBid()
            {
                AuctionId = ev.Bid.AuctionId.ToString(),
                DateCreated = ev.Bid.DateCreated,
                Price = ev.Bid.Price,
                BidId = ev.Bid.BidId.ToString()
            };
            var userFilter =
                Builders<UserRead>.Filter.Eq(f => f.UserIdentity.UserId, ev.Bid.UserIdentity.UserId.ToString());
            var userUpdate = Builders<UserRead>.Update.Push(f => f.UserBids, userBid);

            var result = _dbContext.UsersReadModel.UpdateOne(session, userFilter, userUpdate);
            if (result.MatchedCount == 0)
            {
                throw new QueryException($"Cannot find user with id: {ev.Bid.UserIdentity.UserId.ToString()}");
            }
        }

        public override void Consume(IAppEvent<AuctionRaised> message)
        {
            AuctionRaised ev = message.Event;

            using (var session = _dbContext.Client.StartSession())
            {
                var opt = new TransactionOptions(
                    writeConcern: new WriteConcern(mode: "majority", journal: true)
                );

                try
                {
                    _ = session.WithTransaction((handle, token) =>
                    {
                        UpdateAuction(ev, handle);
                        UpdateUser(ev, handle);
                        return 0;
                    }, transactionOptions: opt);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Cannot update read collections, appEvent: {@appEvent}", message);
                    _requestStatusService.TrySendRequestFailureToUser(message, ev.Bid.UserIdentity);
                    throw;
                }

                _requestStatusService.TrySendReqestCompletionToUser(message, ev.Bid.UserIdentity);
            }
        }
    }
}