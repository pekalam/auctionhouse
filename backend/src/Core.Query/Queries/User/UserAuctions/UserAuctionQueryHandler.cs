using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.User.UserAuctions
{
    public class UserAuctionQueryHandler : IRequestHandler<UserAuctionsQuery, UserAuctionsQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IUserIdentityService _userIdentityService;
        public const int PageSize = 10;

        public UserAuctionQueryHandler(ReadModelDbContext dbContext, IUserIdentityService userIdentityService)
        {
            _dbContext = dbContext;
            _userIdentityService = userIdentityService;
        }

        private UserIdentity GetSignedInUserIdentity()
        {
            var user = _userIdentityService.GetSignedInUserIdentity();
            if (user == null)
            {
                throw new Exception("Not signed in");
            }
            return user;
        }

        public async Task<UserAuctionsQueryResult> Handle(UserAuctionsQuery request, CancellationToken cancellationToken)
        {
            var user = GetSignedInUserIdentity();

            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId, user.UserId.ToString());
            var idsToJoin = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Project(model => new { AuctionsIds = model.CreatedAuctions.Select(s => s).ToArray()})
                .FirstOrDefaultAsync();

            if (idsToJoin != null)
            {
                var userAuctionsFilter = Builders<AuctionRead>.Filter
                    .In(field => field.AuctionId, idsToJoin.AuctionsIds);
                var userAuctions = await _dbContext.AuctionsReadModel
                    .Find(userAuctionsFilter)
                    .Skip(request.Page * PageSize)
                    .Limit(PageSize)
                    .ToListAsync();

                return new UserAuctionsQueryResult() { Auctions = userAuctions };
            }

            return new UserAuctionsQueryResult();
        }
    }
}
