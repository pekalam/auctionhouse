using System;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.Handlers
{
    public class AuctionRaisedHandler : EventConsumer<AuctionRaised>
    {
        private ReadModelDbContext _dbContext;
        private readonly IEventSignalingService _eventSignalingService;
        private readonly ILogger<AuctionRaisedHandler> _logger;

        public AuctionRaisedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            IEventSignalingService eventSignalingService, ILogger<AuctionRaisedHandler> logger) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _eventSignalingService = eventSignalingService;
            _logger = logger;
        }

        private void UpdateAuction(AuctionRaised ev, IClientSessionHandle session)
        {
            var auctionIdFilter = Builders<AuctionReadModel>.Filter.Eq(f => f.AuctionId, ev.Bid.AuctionId.ToString());
            var auctionVersionFilter = Builders<AuctionReadModel>.Filter.Eq(f => f.Version, ev.AggVersion - 1);
            var bidUpdate = Builders<AuctionReadModel>.Update.Set(f => f.WinningBid, ev.Bid);
            var priceUpd = Builders<AuctionReadModel>.Update.Set(f => f.ActualPrice, ev.Bid.Price);
            var totalBidsUpd = Builders<AuctionReadModel>.Update.Inc(f => f.TotalBids, 1);
            var versionUpd = Builders<AuctionReadModel>.Update.Set(f => f.Version, ev.AggVersion);

            try
            {
                var result = _dbContext.AuctionsReadModel.UpdateMany(session,
                    Builders<AuctionReadModel>.Filter.And(new[] {auctionIdFilter, auctionVersionFilter}),
                    Builders<AuctionReadModel>.Update.Combine(new[] {bidUpdate, totalBidsUpd, priceUpd, versionUpd}));
                if (result.MatchedCount == 0)
                {
                    throw new Exception($"No auctions with id: {ev.Bid.AuctionId} and" +
                                        $"Version: {ev.AggVersion}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot update AuctionsReadModel {e.Message}");
                throw;
            }
        }

        private void UpdateUser(AuctionRaised ev, IClientSessionHandle session)
        {
            var userBid = new UserBid()
            {
                AuctionId = ev.Bid.AuctionId.ToString(),
                DateCreated = ev.Bid.DateCreated,
                Price = ev.Bid.Price,
            };
            var userFilter = Builders<UserReadModel>.Filter.Eq(f => f.UserIdentity.UserId, ev.Bid.UserIdentity.UserId.ToString());
            var userUpdate = Builders<UserReadModel>.Update.Push(f => f.UserBids, userBid);
            try
            {
                _dbContext.UsersReadModel.UpdateMany(session, userFilter, userUpdate);
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot update UserReadModel {e.Message}");
                throw;
            }
        }

        public override void Consume(IAppEvent<AuctionRaised> message)
        {
            AuctionRaised ev = message.Event;

            var session = _dbContext.Client.StartSession();
            session.StartTransaction();
            try
            {
                UpdateAuction(ev, session);
                UpdateUser(ev, session);
                session.CommitTransaction();
            }
            catch (Exception)
            {
                session.AbortTransaction();
                _eventSignalingService.TrySendEventFailureToUser(message, ev.Bid.UserIdentity);
                throw;
            }

            _eventSignalingService.TrySendEventCompletionToUser(message, ev.Bid.UserIdentity);
        }
    }
}