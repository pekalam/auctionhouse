using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Queries.User.UserAuctions;
using ReadModel.Contracts.Queries.User.UserBoughtAuctions;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserBoughtAuctions
{
    public class
        UserBoughtAuctionsQueryHandler : QueryHandlerBase<UserBoughtAuctionsQuery, UserBoughtAuctionQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        public const int PageSize = 10;

        public UserBoughtAuctionsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<UserBoughtAuctionQueryResult> HandleQuery(UserBoughtAuctionsQuery request,
            CancellationToken cancellationToken)
        {
            SortDefinition<AuctionRead> sorting = null;
            if (request.Sorting == UserAuctionsSorting.DATE_CREATED)
            {
                sorting = request.SortingDirection == UserAuctionsSortDir.ASCENDING ?
                    Builders<AuctionRead>.Sort.Ascending(read => read.DateCreated) :
                    Builders<AuctionRead>.Sort.Descending(read => read.DateCreated);
            }

            var completedFilter = Builders<AuctionRead>.Filter.Eq(read => read.Completed, true);
            var boughtFilter = Builders<AuctionRead>.Filter.Eq(read => read.Bought, true);
            var buyerFilter =
                Builders<AuctionRead>.Filter.Eq(read => read.Buyer.UserId, request.SignedInUser.ToString());
            var filter = Builders<AuctionRead>.Filter.And(buyerFilter, completedFilter, boughtFilter);

            var count = await _dbContext.AuctionsReadModel.CountDocumentsAsync(filter);

            if (count > 0)
            {
                var boughtAuctions = await _dbContext.AuctionsReadModel
                    .Find(filter)
                    .Sort(sorting)
                    .Skip(PageSize * request.Page)
                    .Limit(PageSize)
                    .ToListAsync();
                return new UserBoughtAuctionQueryResult()
                {
                    Total = count,
                    Auctions = boughtAuctions
                };
            }

            return new UserBoughtAuctionQueryResult();
        }
    }
}