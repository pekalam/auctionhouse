using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Queries.User.UserBids;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserBids
{
    public class UserBidsQueryHandler : QueryHandlerBase<UserBidsQuery, UserBidsQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        public const int PageSize = 10;

        public UserBidsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<UserBidsQueryResult> HandleQuery(UserBidsQuery request,
            CancellationToken cancellationToken)
        {
            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId,
                request.SignedInUser.ToString());
            var result = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Limit(PageSize)
                .Skip(request.Page * PageSize)
                .Project(model => new UserBidsQueryResult()
                {
                    UserBids = model.UserBids
                })
                .FirstOrDefaultAsync();

            if (result != null)
            {
                long total = await _dbContext.UsersReadModel.CountDocumentsAsync(userReadModelFilter);
                result.Total = total;

                var auctionFilter = Builders<AuctionRead>.Filter.In(f => f.AuctionId,
                    result.UserBids.Select(b => b.AuctionId).ToArray());
                var names = await _dbContext.AuctionsReadModel
                    .Find(auctionFilter)
                    .Limit(PageSize)
                    .Skip(request.Page * PageSize)
                    .Project(read => new { read.AuctionId, read.Name })
                    .ToListAsync();

                foreach (var name in names)
                {
                    foreach (var bid in result.UserBids)
                    {
                        if (name.AuctionId == bid.AuctionId)
                        {
                            bid.AuctionName = name.Name;
                        }
                    }
                }
            }



            return result;
        }
    };
}
