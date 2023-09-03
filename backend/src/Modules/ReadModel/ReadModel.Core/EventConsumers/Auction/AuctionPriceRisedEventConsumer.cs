using AuctionBids.DomainEvents;
using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Services;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionPriceRisedEventConsumer : EventConsumer<AuctionPriceRised, AuctionPriceRisedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IBidRaisedNotifications _bidRaisedNotifications;
        private readonly ILogger<AuctionPriceRisedEventConsumer> _logger;

        public AuctionPriceRisedEventConsumer(ILogger<AuctionPriceRisedEventConsumer> logger, EventConsumerDependencies dependencies,
            ReadModelDbContext dbContext, IBidRaisedNotifications bidRaisedNotifications)
            : base(logger, dependencies)
        {
            _logger = logger;
            _dbContext = dbContext;
            _bidRaisedNotifications = bidRaisedNotifications;
        }

        public Task UpdateAuctionRead(IAppEvent<AuctionPriceRised> appEvent, UserIdentityRead userIdentity)
        {
            var update = Builders<AuctionRead>.Update
            .Set(a => a.ActualPrice, appEvent.Event.CurrentPrice)
            .Set(a => a.WinningBid, new()
            {
                AuctionId = appEvent.Event.AuctionId.ToString(),
                BidId = appEvent.Event.BidId.ToString(),
                DateCreated = appEvent.Event.Date,
                Price = appEvent.Event.CurrentPrice,
                UserIdentity = userIdentity,
            })
            .Set(a => a.Winner, userIdentity);
            var idFilter = Builders<AuctionRead>.Filter
                        .Eq(a => a.AuctionId, appEvent.Event.AuctionId.ToString());
            var filter = Builders<AuctionRead>.Filter.And(CategoryFilterFactory.Create(appEvent.Event.CategoryId, appEvent.Event.SubCategoryId, appEvent.Event.SubSubCategoryId), idFilter);

            return _dbContext.AuctionsReadModel.UpdateOneAsync(filter, update);
        }

        public Task<bool> UpdateAuctionBids(IAppEvent<AuctionPriceRised> appEvent)
        {
            var idFilter = Builders<AuctionBidsRead>.Filter.Eq(b => b.AuctionBidsId, appEvent.Event.AuctionBidsId.ToString());
            var versionFilter = Builders<AuctionBidsRead>.Filter
                .Lt(a => a.Version, appEvent.Event.AggVersion);

            var update = Builders<AuctionBidsRead>.Update
                .Set(b => b.WinnerBidId, appEvent.Event.BidId.ToString())
                .Set(b => b.CurrentPrice, appEvent.Event.CurrentPrice)
                .Set(b => b.WinnerId, appEvent.Event.WinnerId.ToString())
                .Set(b => b.Version, appEvent.Event.AggVersion);

            return _dbContext.AuctionBidsReadModel
                .UpdateOneAsync(Builders<AuctionBidsRead>.Filter.And(idFilter, versionFilter), update)
                .ContinueWith(t => t.Result.ModifiedCount > 0,
                    TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task UpdateUserRead(IAppEvent<AuctionPriceRised> appEvent)
        {
            var update = Builders<UserRead>.Update
                .Push(u => u.UserBids, new UserBid
                {
                    AuctionId = appEvent.Event.AuctionId.ToString(),
                    BidId = appEvent.Event.BidId.ToString(),
                    DateCreated = appEvent.Event.Date,
                    Price = appEvent.Event.CurrentPrice,
                });
            var filerBuilder = Builders<UserRead>.Filter;
            var bidsFilter = filerBuilder.Not(filerBuilder.ElemMatch(u => u.UserBids,
                        Builders<UserBid>.Filter.Eq(b => b.BidId, appEvent.Event.BidId.ToString())));
            var idFilter = filerBuilder.Eq(u => u.UserIdentity.UserId, appEvent.Event.WinnerId.ToString());
            var filter = filerBuilder.And(idFilter, bidsFilter);
            return _dbContext.UsersReadModel.UpdateOneAsync(filter, update);
        }

        private async Task<UserIdentityRead> GetUserIdentity(IAppEvent<AuctionPriceRised> appEvent)
        {
            var userProjection = await _dbContext.UsersReadModel
                            .Find(u => u.UserIdentity.UserId == appEvent.Event.WinnerId.ToString())
                            .Project(u => new { Username = u.UserIdentity.UserName }).FirstOrDefaultAsync();
            var userIdentity = new UserIdentityRead()
            {
                UserId = appEvent.Event.WinnerId.ToString(),
                UserName = userProjection.Username,
            };
            return userIdentity;
        }

        public override async Task Consume(IAppEvent<AuctionPriceRised> appEvent) //TODO add Cancellation token
        {
            var userIdentity = await GetUserIdentity(appEvent);

            var transactionSuccess = false;
            using (var session = _dbContext.Client.StartSession())
            {
                var transactionOptions = new TransactionOptions(
                    readPreference: ReadPreference.Primary,
                    readConcern: ReadConcern.Local,
                    writeConcern: WriteConcern.WMajority);

                transactionSuccess = await session.WithTransactionAsync(
                        async (s, ct) =>
                        {
                            await UpdateUserRead(appEvent); //idempotent call
                            var bidsUpdated = await UpdateAuctionBids(appEvent);
                            if (bidsUpdated)
                            {
                                await UpdateAuctionRead(appEvent, userIdentity);
                            }
                            return bidsUpdated;
                        },
                        transactionOptions,
                        CancellationToken.None);
            }

            if (transactionSuccess)
            {
#pragma warning disable CS4014

                //don't await by purpose - it's not essential feature
                try
                {
                    _bidRaisedNotifications.NotifyBidRaised(new(appEvent.Event.AuctionId, appEvent.Event.BidId,
                                appEvent.Event.WinnerId, appEvent.Event.CurrentPrice, appEvent.Event.Date));
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Exception thrown synchronously by {nameof(IBidRaisedNotifications)}");
                }

#pragma warning restore CS4014
            }
        }
    }
}
