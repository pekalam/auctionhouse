using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Auth;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.Queries.User.UserAuctions
{
    public class UserAuctionQueryHandler : QueryHandlerBase<UserAuctionsQuery, UserAuctionsQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        public const int PageSize = 10;

        public UserAuctionQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<UserAuctionsQueryResult> HandleQuery(UserAuctionsQuery request,
            CancellationToken cancellationToken)
        {
            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId,
                request.SignedInUser.UserId.ToString());
            var idsToJoin = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Project(model => new {AuctionsIds = model.CreatedAuctions.Select(s => s).ToArray()})
                .Skip(request.Page * PageSize)
                .Limit(PageSize)
                .FirstOrDefaultAsync();

            if (idsToJoin != null)
            {
                var count = _dbContext.UsersReadModel.AsQueryable()
                    .Where(read => read.UserIdentity.UserId == request.SignedInUser.UserId.ToString())
                    .Select(read => read.CreatedAuctions)
                    .Count();

                var userAuctionsFilter = Builders<AuctionRead>.Filter
                    .In(field => field.AuctionId, idsToJoin.AuctionsIds);
                var userAuctions = await _dbContext.AuctionsReadModel
                    .Find(userAuctionsFilter)
                    .ToListAsync();

                return new UserAuctionsQueryResult() {Auctions = userAuctions, Total = count };
            }

            return new UserAuctionsQueryResult();
        }
    }
}