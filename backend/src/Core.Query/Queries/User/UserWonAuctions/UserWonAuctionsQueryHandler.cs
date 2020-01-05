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
            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId,
                request.SignedInUser.UserId.ToString());
            var idsToJoin = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Skip(request.Page * PageSize)
                .Limit(PageSize)
                .Project(model => new { AuctionsIds = model.WonAuctions.Select(s => s).ToArray() })
                .FirstOrDefaultAsync();

            if (idsToJoin != null)
            {
                var count = _dbContext.UsersReadModel.AsQueryable()
                    .Where(read => read.UserIdentity.UserId == request.SignedInUser.UserId.ToString())
                    .Select(read => read.WonAuctions)
                    .Count();

                var userAuctionsFilter = Builders<AuctionRead>.Filter
                    .In(field => field.AuctionId, idsToJoin.AuctionsIds);
                var userAuctions = await _dbContext.AuctionsReadModel
                    .Find(userAuctionsFilter)
                    .ToListAsync();

                return new UserWonAuctionQueryResult() { Auctions = userAuctions, Total = count };
            }

            return new UserWonAuctionQueryResult();
        }
    }
}