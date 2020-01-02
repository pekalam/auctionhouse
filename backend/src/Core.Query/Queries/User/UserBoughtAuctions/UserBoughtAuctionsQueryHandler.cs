using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.Queries.User.UserAuctions
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
            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId,
                request.SignedInUser.UserId.ToString());
            var idsToJoin = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Skip(request.Page * PageSize)
                .Limit(PageSize)
                .Project(model => new {AuctionsIds = model.BoughtAuctions.Select(s => s).ToArray()})
                .FirstOrDefaultAsync();

            if (idsToJoin != null)
            {
                var count = _dbContext.UsersReadModel.AsQueryable()
                    .Where(read => read.UserIdentity.UserId == request.SignedInUser.UserId.ToString())
                    .Select(read => read.BoughtAuctions)
                    .Count();

                var userAuctionsFilter = Builders<AuctionRead>.Filter
                    .In(field => field.AuctionId, idsToJoin.AuctionsIds);
                var userAuctions = await _dbContext.AuctionsReadModel
                    .Find(userAuctionsFilter)
                    .ToListAsync();

                return new UserBoughtAuctionQueryResult() {Auctions = userAuctions, Total = count};
            }

            return new UserBoughtAuctionQueryResult();
        }
    }
}