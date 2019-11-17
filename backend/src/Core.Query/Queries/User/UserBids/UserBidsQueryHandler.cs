using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.User.UserBids
{
    public class UserBidsQueryHandler : IRequestHandler<UserBidsQuery, UserBidsQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IUserIdentityService _userIdentityService;
        public const int PageSize = 10;

        public UserBidsQueryHandler(ReadModelDbContext dbContext, IUserIdentityService userIdentityService)
        {
            _dbContext = dbContext;
            _userIdentityService = userIdentityService;
        }

        private UserIdentity GetSignedInUserIdentity()
        {
            var user = _userIdentityService.GetSignedInUserIdentity();
            if (user == null)
            {
                throw new Exception("Null user identity");
            }
            return user;
        }

        public async Task<UserBidsQueryResult> Handle(UserBidsQuery request, CancellationToken cancellationToken)
        {
            var user = GetSignedInUserIdentity();

            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId, user.UserId.ToString());
            var result = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Project(model => new UserBidsQueryResult(){UserBids = model.UserBids})
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
