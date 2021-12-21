using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Query.Mediator;
using Core.Query.Queries.User.UserAuctions;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.Queries.User.UserWonAuctions
{
    public class
        UserWonAuctionsQueryHandler : QueryHandlerBase<UserWonAuctionsQuery, UserWonAuctionQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        public const int PageSize = 10;

        public UserWonAuctionsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<UserWonAuctionQueryResult> HandleQuery(UserWonAuctionsQuery request,
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
            var boughtFilter = Builders<AuctionRead>.Filter.Eq(read => read.Bought, false);
            var buyerFilter =
                Builders<AuctionRead>.Filter.Eq(read => read.Buyer.UserId, request.SignedInUser.ToString());
            var filter =
                Builders<AuctionRead>.Filter.And(buyerFilter, completedFilter, boughtFilter);

            var count = await _dbContext.AuctionsReadModel.CountDocumentsAsync(filter);

            if (count > 0)
            {
                var wonAuctions = await _dbContext.AuctionsReadModel
                    .Find(filter)
                    .Sort(sorting)
                    .Skip(PageSize * request.Page)
                    .Limit(PageSize)
                    .ToListAsync();
                return new UserWonAuctionQueryResult()
                {
                    Total = count,
                    Auctions = wonAuctions
                };
            }
            return new UserWonAuctionQueryResult();
        }
    }
}