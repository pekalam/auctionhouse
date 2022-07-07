using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserAuctions
{
    public class UserAuctionsQueryHandler : QueryHandlerBase<UserAuctionsQuery, UserAuctionsQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        public const int PageSize = 10;

        public UserAuctionsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<UserAuctionsQueryResult> HandleQuery(UserAuctionsQuery request,
            CancellationToken cancellationToken)
        {
            SortDefinition<AuctionRead> sorting = null;
            if (request.Sorting == UserAuctionsSorting.DATE_CREATED)
            {
                sorting = request.SortingDirection == UserAuctionsSortDir.ASCENDING ?
                    Builders<AuctionRead>.Sort.Ascending(read => read.DateCreated) :
                    Builders<AuctionRead>.Sort.Descending(read => read.DateCreated);
            }

            FilterDefinition<AuctionRead> filter = null;
            var idfilter =
                Builders<AuctionRead>.Filter.Eq(read => read.Owner.UserId, request.SignedInUser.ToString());

            if (request.ShowArchived)
            {
                filter = idfilter;
            }
            else
            {
                var archivedFilter = Builders<AuctionRead>.Filter.Eq(read => read.Archived, false);
                filter = Builders<AuctionRead>.Filter.And(idfilter, archivedFilter);
            }


            var count = await _dbContext.AuctionsReadModel.CountDocumentsAsync(filter);

            if (count > 0)
            {
                var userAuctions = await _dbContext.AuctionsReadModel
                    .Find(filter)
                    .Sort(sorting)
                    .Skip(PageSize * request.Page)
                    .Limit(PageSize)
                    .ToListAsync();
                return new UserAuctionsQueryResult() { Auctions = userAuctions, Total = count };
            }
            return new UserAuctionsQueryResult();
        }
    }
}